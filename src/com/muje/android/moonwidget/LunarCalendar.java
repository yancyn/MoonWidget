package com.muje.android.moonwidget;

import java.io.IOException;
import java.util.*;

import org.xmlpull.v1.XmlPullParser;
import org.xmlpull.v1.XmlPullParserException;

import android.content.Context;
import android.content.res.Resources;
import android.content.res.XmlResourceParser;
import android.util.Log;

public class LunarCalendar {
	
	/**
	 * A collection of lunar entry from datasheet.
	 * @see res/xml/moon.xml
	 */
	private ArrayList<Lunar> data;
	/**
	 * Today lunar object.
	 */
	private Lunar today;
	/**
	 * Today date in Gregorian value.
	 */
	private Date todaySun;

	/**
	 * Lunar calendar manager class.
	 */
	public LunarCalendar() {
		this.todaySun = new Date();
		this.data = new ArrayList<Lunar>();
	}

	/**
	 * Extract xml into readable collection object.
	 * 
	 * @param activity
	 * @return
	 * @throws XmlPullParserException
	 * @throws IOException
	 * @see http
	 *      ://android-er.blogspot.com/2010/04/read-xml-resources-in-android-
	 *      using.html
	 * @see res/xml/moon.xml
	 */
	public void initialize(Context context) throws XmlPullParserException, IOException {

		Resources res = context.getResources();
		XmlResourceParser xpp = res.getXml(R.xml.moon);
		xpp.next();
		int eventType = xpp.getEventType();

		String sun = "";
		int month = 0;
		String term = "";
		while (eventType != XmlPullParser.END_DOCUMENT) {

			if (eventType == XmlPullParser.START_TAG) {
				for (int i = 0; i < xpp.getAttributeCount(); i++) {
					String name = xpp.getAttributeName(i);
					if (name.compareTo("sun") == 0) {
						sun = xpp.getAttributeValue(i);
					} else if (name.compareTo("month") == 0) {
						month = Integer.parseInt(xpp.getAttributeValue(i));
					} else if (name.compareTo("term") == 0) {
						term = xpp.getAttributeValue(i);
					}
				}
			}

			if (month > 0 || term.length() > 0) {

				Lunar entry = null;
				int year = Integer.parseInt(sun.subSequence(0, 4).toString());
				if (month > 0) {
					entry = new Lunar(year, month, 1);
				} else {
					entry = new Lunar(year, term);
				}

				// Log.d("DEBUG","Year: "+sun.subSequence(0, 4).toString());
				// TODO: Date gregorian = new GregorianCalendar()
				Date gregorian = new Date(
						year - 1900,
						Integer.parseInt(sun.subSequence(5, 7).toString()) - 1,
						Integer.parseInt(sun.subSequence(8, 10).toString()));
				// Log.d("DEBUG","Gregorian: "+gregorian.getYear());
				entry.setSun(gregorian);
				if (term.length() > 0) {
					entry.setTerm(term);
				}
				this.data.add(entry);
			}

			eventType = xpp.next();
			sun = "";
			month = 0;
			term = "";
		}// end loop xml file

		// ensure sort correctly for loop in getToday()
		Collections.sort(this.data, new LunarComparator());
	}

	/**
	 * Return today in lunar date.
	 * 
	 * @return
	 */
	public Lunar getToday() {

		if (today == null) {
			int days = 0;
			Date last = new Date();
			int nextMonth = 0;
			for (int i = 0; i < this.data.size(); i++) {
				// for(Lunar entry: this.data){
				Lunar entry = data.get(i);
				Date sun = entry.getSun();
				if (sun.compareTo(todaySun) >= 0 && entry.getMonth() > 0) {
					Log.d("DEBUG", "last 1st moon: " + last.toString());
					days = diffDays(todaySun, last);
					Log.d("DEBUG", "Diff days: " + days);
					nextMonth = getNextLunarMonth(i);
					break;
				}

				// only take valid Lunar value which has first of moon
				if (entry.getMonth() > 0)
					last = sun;
			}

			Log.d("DEBUG", "Today sun: " + todaySun.toString());
			this.today = new Lunar(last.getYear(), last.getMonth() + 1, days + 1);// include today
			this.today.setTerm(getTerm());
			// Log.d("DEBUG", "Today moon: "+today.toString());

			// check is a leap month or not
			if (this.today.getMonth() == nextMonth) {
				this.today.setLeapMonth();
			}
		}

		return today;
	}
	/**
	 * Get solar term in database.
	 * @return
	 */
	private String getTerm() {
		
		String term = "";
		for(Lunar entry: this.data){
			if(todaySun.compareTo(entry.getSun())==0) {
				return entry.getTerm();
			}
		}
		
		return term;		
	}

	/**
	 * Compare two date values and return the different in days.
	 * 
	 * @param date1
	 * @param date2
	 * @return
	 */
	private int diffDays(Date date1, Date date2) {
		long diffInSecs = (date1.getTime() - date2.getTime()) / 1000;
		return (int) (diffInSecs / 86400);// one day has 86,400 seconds
	}

	/**
	 * Get the following lunar month.
	 * 
	 * @param index
	 *            Index in data collection to start seeking.
	 * @return Lunar month value.
	 */
	private int getNextLunarMonth(int index) {

		for (int i = index; i < this.data.size(); i++) {
			int month = this.data.get(i).getMonth();
			if (month > 0)
				return month;
		}

		return 0;
	}

	/**
	 * Get the next full moon or new moon.
	 * 
	 * @deprecated
	 * @return
	 */
	public String getNext() {

		String result = "";
		return result;
	}
}