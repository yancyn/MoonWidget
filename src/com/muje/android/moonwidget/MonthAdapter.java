package com.muje.android.moonwidget;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;

import org.xmlpull.v1.XmlPullParserException;

import android.R.color;
import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.TextView;

public class MonthAdapter extends BaseAdapter {
	private Context context;
	private LunarCalendar lunarCalendar;
	private AppointmentManager appointmentManager;
	
	/**
	 * Stopper to prevent highlight same date twice.
	 */
	private int firstDayCounter;
	/**
	 * Gregorian year value.
	 */
	private int year;
	/**
	 * Gregorian month value.
	 */
	private int month;	
	/**
	 * For holding purpose.
	 */
	private Lunar lunar;
	/**
	 * Retrieving 24 stems, Buddhist, religion, & etc.
	 */
	private ArrayList<Lunar> events;

	/**
	 * Recommended constructor.
	 * @param context
	 * @param year Gregorian year value. ie. 2012.
	 * @param month Gregorian month integer start from 1,2,3,..,12.
	 * @throws IOException 
	 * @throws XmlPullParserException 
	 */
	public MonthAdapter(Context context,int year, int month) throws XmlPullParserException, IOException {
		this.context = context;
		this.year = year;
		this.month = month;
		this.firstDayCounter = 0;
		
		// load default system lunar calendar
		lunarCalendar = new LunarCalendar();
		lunarCalendar.initialize(context);
		
		// load personal lunar appointment
		appointmentManager = new AppointmentManager();
		appointmentManager.initialize(context);
	}
	/**
	 * Constructor for unit testing.
	 * @param year Gregorian year value. ie. 2012.
	 * @param month Gregorian month integer start from 1,2,3,..,12.
	 */
	public MonthAdapter(int year, int month) {
		this.year = year;
		this.month = month;
	}

	@Override
	public int getCount() {
		return getRowOfWeek(year,month)*7;
	}

	@Override
	public Object getItem(int arg0) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public long getItemId(int arg0) {
		// TODO Auto-generated method stub
		return 0;
	}

	@Override
	public View getView(int position, View convertView, ViewGroup arg2) {
		
		if(events == null) {
			events = lunarCalendar.getEvents(this.year,this.month);
		}

		if (convertView == null) {
			
			// define how the view to be display in grid layout
			LayoutInflater vi = (LayoutInflater) this.context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);// key
			convertView = vi.inflate(R.layout.dayblock, null);
			
			int i = getFirstDayOfMonth();
			Date lastDay = getLastDayOfMonth();
			if(position >= i) {		
				
				//change the numbering of day in month
				int date = (position-i)%lastDay.getDate();
				
				// current date value of block
				Date day = new Date(this.year-1900,this.month-1,date+1);
				
				//not to draw next month
				if(day.getDate() == 1) firstDayCounter ++;
				if(firstDayCounter > 1) return convertView;
				
				// draw gregorian date
				TextView textViewDate = (TextView)convertView.findViewById(R.id.textViewDate);
				textViewDate.setText(Integer.toString(day.getDate()));//Integer.toString(date+1));
				Log.d("DEBUG", "getView("+position+"): "+day.toString());
				
				// highlight today in grey background
				Date today = new Date(new Date().getYear(), new Date().getMonth(), new Date().getDate());
				if(firstDayCounter == 1 && day.compareTo(today) == 0) {
					convertView.setBackgroundResource(R.drawable.todayview);
				}				
				
				// map lunar to gregorian date				
				lunar = lunarCalendar.getLunar(day);
				lunar.setEvents(lunarCalendar.getEvents(lunar));
				if(lunar == null) return convertView;
				
				//get 24 stem
				String text = "";
				text = lunar.getTerm();
				
				// get personal lunar event
				String desc = "";
				for(Appointment appointment:lunar.getEvents()) {
					if(desc.length()>0) desc += "\n";
					desc += appointment.getDescription();
				}//end loops
				
				if(desc != "") {
					//show personal lunar appointment
					TextView textViewLunarEvent = (TextView)convertView.findViewById(R.id.textViewLunarEvent);
					textViewLunarEvent.setText(desc);
					// FAIL: textViewLunarEvent.setBackgroundColor(color.holo_orange_dark);
					textViewLunarEvent.setBackgroundResource(R.color.highlight);
				}
				
				// Start set text into interface				
				TextView textViewLunarDate = (TextView)convertView.findViewById(R.id.textViewLunarDate);
				if(text != "") {					
					textViewLunarDate.setText(text);
				} else {					
					//show lunar date
					if(lunar.getDay() == 1) {
						textViewLunarDate.setText(lunar.getMonthText());
					} else {
						textViewLunarDate.setText(lunar.getDayText());
					}					
				}
				Log.d("DEBUG",lunar.toString());
			}
		}

		return convertView;
	}
	
	
	/*
	 * Decide how many row need to draw for this month if first day of month
	 * fall at last day of the week or last day in a month fall at first day of
	 * week then need to draw 6 rows otherwise only need 5 rows.
	 * The week begin with Sunday.
	 */
	public int getRowOfWeek(int year,int month) {

		int rowOfWeek = 5;// default
		
		//Date today = new Date();
		Date firstDay = new Date(year-1900,month-1,1);
		
		Calendar calendar = Calendar.getInstance();
		calendar.set(year, month-1, 1);//calendar.JANUARRY = 0
		int totalDayInMonth = calendar.getActualMaximum(calendar.DAY_OF_MONTH);
		Date lastDay = new Date(year-1900,month-1,totalDayInMonth);
		
		// sunday = 0, monday = 1, ... , saturday = 6
		if(firstDay.getDay() == 6 || lastDay.getDay() == 0)
			rowOfWeek ++;

		return rowOfWeek;
	}
	/**
	 * Return the weekday of 1st of month.
	 * Start from Sunday = 0, Monday = 1.. , Saturday = 6.
	 * @return
	 */
	public int getFirstDayOfMonth() {
		Date firstDay = new Date(year-1900,month-1,1);
		return firstDay.getDay();
	}
	/**
	 * Return last date in a month ie. 31 or 30th.
	 * @return
	 */
	public Date getLastDayOfMonth() {
		Calendar calendar = Calendar.getInstance();
		calendar.set(year, month-1, 1);//calendar.JANUARRY = 0
		int totalDayInMonth = calendar.getActualMaximum(calendar.DAY_OF_MONTH);
		return new Date(year-1900,month-1,totalDayInMonth);
	}

}