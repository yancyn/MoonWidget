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

	private ArrayList<Lunar> data;
	private Lunar today;

	/**
	 * Lunar calendar manager class.
	 */
	public LunarCalendar() {
		this.data = new ArrayList<Lunar>();
	}

	/**
	 * Extract xml into readable collection object.
	 * TODO: Ensure sorting must correct in source file.
	 * @param activity
	 * @return
	 * @throws XmlPullParserException
	 * @throws IOException
	 * @see http://android-er.blogspot.com/2010/04/read-xml-resources-in-android-using.html
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
				if(month > 0) {
					entry = new Lunar(year, month, 1);
				} else {
					entry = new Lunar(year,term);
				}
				
				//Log.d("DEBUG","Year: "+sun.subSequence(0, 4).toString());
//				TODO: Date gregorian = new GregorianCalendar()
				Date gregorian = new Date(
						year-1900,
						Integer.parseInt(sun.subSequence(5, 7).toString())-1,
						Integer.parseInt(sun.subSequence(8, 10).toString()));
				//Log.d("DEBUG","Gregorian: "+gregorian.getYear());
				entry.setSun(gregorian);				
				if(term.length()>0) {
					entry.setTerm(term);
				}
				this.data.add(entry);
			}

			eventType = xpp.next();
			sun = "";
			month = 0;
			term = "";
		}//end loop xml file
		
		//ensure sort correctly for loop in getToday()
		Collections.sort(this.data,new LunarComparator());
	}

	/**
	 * Return today in  lunar date.
	 * 
	 * @return
	 */
	public Lunar getToday() {

		if (today == null) {
			Date todaySun = new Date();
			Date last = new Date();
			int days = 1;
			for(int i=0;i<this.data.size();i++) {
			//for(Lunar entry: this.data){
				Lunar entry = data.get(i);
				Date sun = entry.getSun();
				if(sun.compareTo(todaySun) >= 0 && entry.getMonth() > 0) {
					Log.d("DEBUG","last 1st moon: " + last.toString());
					long diffInSecs = (todaySun.getTime()-last.getTime())/1000;
					days += (int)(diffInSecs/86400);//one day has 86,400 seconds
					Log.d("DEBUG","Diff days: "+days);
					break;
				}
				
				//only take valid Lunar value which has first of moon
				if(entry.getMonth()>0) last = sun;
			}
			
			Log.d("DEBUG", "Today sun: " + todaySun.toString());
			this.today = new Lunar(last.getYear(), last.getMonth() + 1, days);
			//Log.d("DEBUG", "Today moon: "+today.toString());
		}

		return today;
	}
	/**
	 * Get the next full moon or new moon.
	 * @deprecated
	 * @return
	 */
	public String getNext() {

		String result = "";
		return result;
	}
}