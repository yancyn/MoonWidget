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

	public ArrayList<Lunar> data;
	public Lunar today;

	public LunarCalendar() {
		this.data = new ArrayList<Lunar>();
	}

	/**
	 * TODO: Extract xml into readable collection object.
	 * 
	 * @param activity
	 * @return
	 * @throws XmlPullParserException
	 * @throws IOException
	 * @see http
	 *      ://android-er.blogspot.com/2010/04/read-xml-resources-in-android-
	 *      using.html
	 */
	public void initialize(Context context) throws XmlPullParserException,
			IOException {

		Resources res = context.getResources();
		XmlResourceParser xpp = res.getXml(R.xml.moon);
		xpp.next();
		int eventType = xpp.getEventType();

		String sun = "";
		int month = 0;
		String term = "";
		Date now = new Date();
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

			if (month > 0) {
				this.data.add(new Lunar(now.getYear(), month, 1));
			}

			eventType = xpp.next();
			sun = "";
			month = 0;
			term = "";
		}
	}

	/**
	 * Return today lunar date.
	 * 
	 * @return
	 */
	public Lunar getToday() {

		if (today == null) {
			Date todaySun = new Date();
			Log.d("DEBUG", "Today: " + todaySun.toString());
			this.today = new Lunar(todaySun.getYear(), todaySun.getMonth() + 1,
					todaySun.getDate());
		}

		return today;
	}

	public String getNext() {

		String result = "";
		return result;
	}
}