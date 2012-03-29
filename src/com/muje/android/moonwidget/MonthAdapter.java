package com.muje.android.moonwidget;

import java.util.Calendar;
import java.util.Date;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.TextView;

public class MonthAdapter extends BaseAdapter {
	private Context context;
	/**
	 * Gregorian year value.
	 */
	private int year;
	/**
	 * Gregorian month value.
	 */
	private int month;

	/**
	 * Recommended constructor.
	 * @param context
	 * @param year Gregorian year value. ie. 2012.
	 * @param month Gregorian month integer start from 1,2,3,..,12.
	 */
	public MonthAdapter(Context context,int year, int month) {
		this.context = context;
		this.year = year;
		this.month = month;
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

		if (convertView == null) {
			
			// define how the view to be display in grid layout
			LayoutInflater vi = (LayoutInflater) this.context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);// key
			convertView = vi.inflate(R.layout.dayblock, null);
			
			int i = getFirstDayOfMonth();
			Date lastDay = getLastDayOfMonth();
			if(i==0) i += 7;//because Sunday is start from index [6]
			if(position >= i-1) {		
				
				//change the numbering of day in month
				int date = (position-i+2)%(lastDay.getDate()+1);
				if(date==0) date++;//date is count from 1 not 0
				
				TextView textViewDate = (TextView)convertView.findViewById(R.id.textViewDate);
				textViewDate.setText(Integer.toString(date));
				
				//TODO: map lunar to gregorian date
				TextView textViewLunarDate = (TextView)convertView.findViewById(R.id.textViewLunarDate);
				textViewLunarDate.setText(Lunar.DAYS[(date-1)%Lunar.DAYS.length]);
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