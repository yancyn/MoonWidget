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
	Context context;

	public MonthAdapter(Context context) {
		this.context = context;
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
			TextView textView11 = (TextView)convertView.findViewById(R.id.textView11);
			textView11.setText(Integer.toString(position));
			
			TextView textView12 = (TextView)convertView.findViewById(R.id.textView12);
			textView12.setText(Lunar.DAYS[position%Lunar.DAYS.length]);			
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
		
		Date today = new Date();
		Date firstDay = new Date(
				today.getYear(),
				today.getMonth(),
				1);
		Calendar calendar = Calendar.getInstance();
		int totalDayInMonth = calendar.getActualMaximum(calendar.DAY_OF_MONTH);
		Date lastDay = new Date(
				today.getYear(),
				today.getMonth(),
				totalDayInMonth);
		// sunday = 0, monday = 1, ... , saturday = 6
		if(firstDay.getDay() == 0 || lastDay.getDay() == 1)
			rowOfWeek ++;

		return rowOfWeek;
	}

}