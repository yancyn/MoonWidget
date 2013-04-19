package com.muje.android.moonwidget;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;

import org.xmlpull.v1.XmlPullParserException;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.TextView;

public class MonthAdapter extends BaseAdapter {
	private Context context;
	private LunarCalendar lunarCalendar;
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
	 * Retrieving 24 stem, Buddhist, religion, & etc.
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
		
		lunarCalendar = new LunarCalendar();
		lunarCalendar.initialize(context);
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
			//if(i==0) i += 1;//because Sunday is start from index [0]
			if(position >= i) {		
				
				//change the numbering of day in month
				int date = (position-i)%lastDay.getDate();
				Date day = new Date(this.year-1900,this.month-1,date+1);
				
				//not to draw next month
				if(day.getDate() == 1) firstDayCounter ++;
				if(firstDayCounter > 1) return convertView;
				
				//draw gregorian date
				TextView textViewDate = (TextView)convertView.findViewById(R.id.textViewDate);
				textViewDate.setText(Integer.toString(date+1));
				
				//highlight today in grey background
				Date today = new Date(new Date().getYear(),
						new Date().getMonth(),
						new Date().getDate());
				if(firstDayCounter == 1 && day.compareTo(today) == 0) {
					convertView.setBackgroundResource(R.drawable.todayview);
				}
				
				
				// map lunar to gregorian date				
				if(lunar == null) lunar = lunarCalendar.getLunar(day);
				if(lunar == null) return convertView;
				
				int lenghtOfLunarMonth = lunarCalendar.diffDays(
						lunarCalendar.getNextNewMoon().getSun(),
						lunarCalendar.getCurrentNewMoon().getSun());				
				int index = ((lunar.getDay()+date-1))%Math.min(lenghtOfLunarMonth,Lunar.DAYS.length);				
				
				//get 24 stem
				String text = "";				
				for(Lunar event:events) {
					if(event.getSun().compareTo(day) == 0) {
						text = event.getTerm();
					}
				}
				
				TextView textViewLunarDate = (TextView)convertView.findViewById(R.id.textViewLunarDate);
				if(text != "") {					
					textViewLunarDate.setText(text);
				} else {
					if(index==0) {
						Lunar nextMoon = lunarCalendar.getNextNewMoon();					
						textViewLunarDate.setText(nextMoon.getMonthText());
					} else {
						textViewLunarDate.setText(Lunar.DAYS[index]);
					}
					
				}
			}
		}

		return convertView;
	}
	
	
	/*
	 * Decide how many row need to draw for this month if first day of month
	 * fall at last day of the week or last day in a month fall at first day of
	 * week then need to draw 6 rows otherwise only need 5 rows.
	 * The week begin with Monday.
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
		if(firstDay.getDay() == 0 || lastDay.getDay() == 1)
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