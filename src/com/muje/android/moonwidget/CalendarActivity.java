package com.muje.android.moonwidget;

import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;

import org.xmlpull.v1.XmlPullParserException;

import android.app.Activity;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.text.format.DateUtils;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.GridView;
import android.widget.TextView;
import android.widget.ImageView;

/**
 * Lunar calendar view activity.
 * 
 * @author yeang-shing.then
 * @see http://www.firstdroid.com/2011/02/06/android-tutorial-gridview-with-icon-and-text/
 */
public class CalendarActivity extends Activity {
	private GridView gridViewCalendar;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.calendar);
		
		//previous month click
		Button button1 = (Button)findViewById(R.id.buttonPrevious);
		button1.setOnClickListener(previousOnClick);
		
		//next month click
		Button button2 = (Button)findViewById(R.id.buttonNext);
		button2.setOnClickListener(nextOnClick);
		
		//show this month
		Calendar today = Calendar.getInstance();
		display(today.getTime().getYear()+1900,
				today.getTime().getMonth()+1);
	}
	/**
	 * Display calendar in month view.
	 * @param year Gregorian year value ie. 2012
	 * @param month Gregorian month value from 1 to 12.
	 */
	private void display(int year, int month) {
		
		//validation
		if(month>12) {
			year++;
			month -= 12;
		} else if(month<=0) {
			year--;
			month += 12;
		}
		
		//display Chinese Year
		Lunar lunar = new Lunar(year,month,1);
		TextView textViewYear = (TextView)findViewById(R.id.textViewYear);
		textViewYear.setText(lunar.getYearText());
		
		// set animal avatar
		ImageView imageView = (ImageView)findViewById(R.id.imageViewAnimal);
		Drawable resource = getResources().getDrawable(lunar.getAnimalYearId());
		imageView.setImageDrawable(resource);
		
		// display this month as text
		Date date = new Date(year-1900,month-1,1);
		Log.d("DEBUG","displaying month: "+date.toLocaleString());
		SimpleDateFormat format = new SimpleDateFormat("MMM yyyy");
		TextView textViewMonth = (TextView) findViewById(R.id.textViewMonth);
		textViewMonth.setText(format.format(date));
		//for next & previous button recognize what is current displayed month
		textViewMonth.setTag(date);
		
		// set weekday header in local language
		int i = 0;
		int[] headerIds = new int[]{R.id.sun, R.id.mon, R.id.tue, R.id.wed, R.id.thu, R.id.fri, R.id.sat};
		for(int id: headerIds) {
			TextView header = (TextView)findViewById(id);
			header.setText(DateUtils.getDayOfWeekString(i+1, 3));
			i++;
		}
		

		// arrange all day in a month view
		gridViewCalendar = (GridView) findViewById(R.id.gridViewCalendar);		
		try {
			gridViewCalendar.setAdapter(new MonthAdapter(this,year,month));
		} catch (XmlPullParserException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	
	protected OnClickListener previousOnClick = new OnClickListener() {

		@Override
		public void onClick(View arg0) {
			TextView textViewMonth = (TextView)findViewById(R.id.textViewMonth);
			Date date = (Date)textViewMonth.getTag();
			display(date.getYear()+1900,date.getMonth());			 
		}		
	};
	protected OnClickListener nextOnClick = new OnClickListener() {

		@Override
		public void onClick(View v) {
			TextView textViewMonth = (TextView)findViewById(R.id.textViewMonth);
			Date date = (Date)textViewMonth.getTag();
			display(date.getYear()+1900,date.getMonth()+2);			
		}
		
	};
}
