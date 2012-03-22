package com.muje.android.moonwidget;

import java.text.SimpleDateFormat;
import java.util.Calendar;

import android.app.Activity;
import android.os.Bundle;
import android.widget.GridView;
import android.widget.TextView;

/**
 * Lunar calendar view activity.
 * 
 * @author yeang-shing.then
 * @see http
 *      ://www.firstdroid.com/2011/02/06/android-tutorial-gridview-with-icon-
 *      and-text/
 */
public class CalendarActivity extends Activity {
	private GridView gridView1;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.calendar);

		//display this month as text
		SimpleDateFormat format = new SimpleDateFormat("MMM");
		Calendar today = Calendar.getInstance();		
		TextView textView1 = (TextView) findViewById(R.id.textView1);
		textView1.setText(format.format(today.getTime()));

		//arrange all day in a month view
		gridView1 = (GridView) findViewById(R.id.gridView1);
		gridView1.setAdapter(new MonthAdapter(this));
	}
}
