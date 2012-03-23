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
	private int year;
	private int month;

	/**
	 * Default constructor.
	 * @param context
	 * @param year Gregorian year value. ie. 2012.
	 * @param month Gregorian month integer start from 1,2,3,..,12.
	 */
	public MonthAdapter(Context context,int year, int month) {
		this.context = context;
		this.year = year;
		this.month = month;
	}

	@Override
	public int getCount() {
		return getRowOfWeek()*7;
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
			
			//TODO: change the numbering of day in month
			TextView textViewDate = (TextView)convertView.findViewById(R.id.textViewDate);
			textViewDate.setText(Integer.toString(position));
			
			TextView textViewLunarDate = (TextView)convertView.findViewById(R.id.textViewLunarDate);
			textViewLunarDate.setText(Lunar.DAYS[position%Lunar.DAYS.length]);			
		}

		return convertView;
	}
	
	/*
	 * Decide how many row need to draw for this month if first day of month
	 * fall at last day of the week or last day in a month fall at first day of
	 * week then need to draw 6 rows otherwise only need 5 rows.
	 * The week begin with Monday.
	 */
	private int getRowOfWeek() {

		int rowOfWeek = 5;// default

//		Calendar today = Calendar.getInstance();
//		// get first day in a month
//		Calendar firstDay = Calendar.getInstance();
//		firstDay.set(today.YEAR-1900, today.MONTH, 1);today.DATE
//		// get last day in a month
//		int lastDate = today.getActualMaximum(today.DAY_OF_MONTH);
//		Calendar lastDay = Calendar.getInstance();
//		lastDay.set(today.getTime().getYear()-1900, today.MONTH, lastDate);
//		SimpleDateFormat format = new SimpleDateFormat("F");
//		if(format.format(firstDay) == "0"
//				|| format.format(lastDay) == "1")
//			rowOfWeek++;
		
		//Date today = new Date();
		Date firstDay = new Date(year-1900,month-1,1);
		
		Calendar calendar = Calendar.getInstance();
		calendar.set(year, month, 1);
		int totalDayInMonth = calendar.getActualMaximum(calendar.DAY_OF_MONTH);
		Date lastDay = new Date(year-1900,month-1,totalDayInMonth);
		
		// sunday = 0, monday = 1, ... , saturday = 6
		if(firstDay.getDay() == 0 || lastDay.getDay() == 1)
			rowOfWeek ++;

		return rowOfWeek;
	}

}