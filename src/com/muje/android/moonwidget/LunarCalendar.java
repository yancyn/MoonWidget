package com.muje.android.moonwidget;

import java.io.IOException;
import java.util.*;

import org.xmlpull.v1.XmlPullParser;
import org.xmlpull.v1.XmlPullParserException;

import android.content.Context;
import android.content.res.Resources;
import android.content.res.XmlResourceParser;

public class LunarCalendar {

	public ArrayList<Lunar> Data;

	public LunarCalendar() {
		this.Data = new ArrayList<Lunar>();
	}

	/**
	 * TODO:Extract xml into readable collection object.
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

		// String source = "";
		// String content = "";
		while (eventType != XmlPullParser.END_DOCUMENT) {

			if (eventType == XmlPullParser.START_TAG) {
				for (int i = 0; i < xpp.getAttributeCount(); i++) {
					String name = xpp.getAttributeName(i);
					if (name.compareTo("from") == 0) {
						// source = xpp.getAttributeValue(i);
					}
				}
			} else if (eventType == XmlPullParser.TEXT) {
				// content = xpp.getText();

				// Quote quote = new Quote(source, content);
				// this.Quotes.add(quote);

				// source = "";// reset
				// content = "";// reset
			}

			eventType = xpp.next();
		}
	}
	
	/**
	 * Return today lunar date.
	 * @return
	 */
	public Lunar getToday() {

		Date today = new Date();
		Lunar result = new Lunar(today.getYear(), today.getMonth(), today.getDay());
		return result;
	}

	public String getNext() {

		String result = "";
		return result;
	}
}
