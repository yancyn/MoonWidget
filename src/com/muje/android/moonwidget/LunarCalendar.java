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
	private AppointmentManager appointmentManager;
	/**
	 * Today lunar object.
	 */
	private Lunar today;
	/**
	 * Today date in Gregorian value.
	 */
	private Date todaySun;
	
	private Lunar currentNewMoon;
	/**
	 * Return lunar date for previous new moon.
	 * @see getLunar()
	 */
	public Lunar getCurrentNewMoon() {return this.currentNewMoon;}
	
	private Lunar nextNewMoon;
	/**
	 * Return lunar date for next new moon.
	 * @see getLunar()
	 */
	public Lunar getNextNewMoon() {return this.nextNewMoon;}

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
				if (month > 0) {
					entry = new Lunar(year, month, 1);
				} else {
					entry = new Lunar(year, term);
				}

				// Log.d("DEBUG","Year: "+sun.subSequence(0, 4).toString());
				// TODO: change to Date gregorian = new GregorianCalendar()
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
		
		//initialize personal events in database
		appointmentManager = new AppointmentManager();
		appointmentManager.initialize(context);
	}
	/**
	 * Return lunar date.
	 * 
	 * @return
	 */
	public Lunar getLunar(Date gregorian) {
		
		Lunar result = null;

		int days = 0;
		Lunar last = null;
		ArrayList<Lunar> months = new ArrayList<Lunar>();
		for (int i = 0; i < this.data.size(); i++) {
			Lunar entry = data.get(i);
			if (entry.getSun().compareTo(gregorian) > 0 && entry.getMonth() > 0) {
				
				this.currentNewMoon = last;
				this.nextNewMoon = getNextNewMoon(i);				
				if(last != null) {
					Log.i("LunarCalendar.getLunar", "last 1st moon: " + last.toString());
					days = diffDays(gregorian, last.getSun());
					Log.i("LunarCalendar.getLunar", "Diff days: " + days);
				}				
				break;
			}

			// only take valid Lunar value which has first of moon
			if (entry.getMonth() > 0) {
				last = entry;
				months.add(entry);
			}
		}
		
		// check is a leap month or not
		int size = months.size();
		if(size==0) return result;		
		if(size < 2) {
			result = new Lunar(last.getSunYear(), last.getMonth(), days + 1);// include today
		} else {
			Lunar lastMonth = months.get(size-1);
			Lunar lastTwoMonth = months.get(size-2);
			if(lastMonth.getMonth() == lastTwoMonth.getMonth()) {
				result = new Lunar(last.getSunYear(), last.getMonth(), days + 1);// include today
				result.setLeapMonth();
			} else {
				result = new Lunar(last.getSunYear(), last.getMonth(), days + 1);// include today
			}
			result.setTerm(getTerm(gregorian));
		}
		
		Log.i("LunarCalendar.getLunar",gregorian.toLocaleString()+ " equals " +result.toString());
		result.setEvents(getEvents(result));
		return result;
	}
	/**
	 * Get solar term in database.
	 * @param gregorian
	 * @return
	 */
	private String getTerm(Date gregorian) {
		
		String term = "";
		for(Lunar entry: this.data){
			if(gregorian.compareTo(entry.getSun())==0) {
				return entry.getTerm();
			}
		}
		
		return term;		
	}
	/**
	 * Return a collection of result by month.
	 * @param year
	 * @param month
	 * @return
	 */
	public ArrayList<Lunar> getEvents(int year, int month) {
		
		ArrayList<Lunar> output = new ArrayList<Lunar>();
		for(Lunar entry:this.data) {
			if(entry.getSun().getYear()+1900 == year
					&& entry.getSun().getMonth()+1 == month)
				output.add(entry);
		}
		
		return output;
	}
	/**
	 * Return a collection of personal events.
	 * @param year
	 * @param month
	 * @return
	 */
	private ArrayList<Appointment> getEvents(Lunar lunar) {
		ArrayList<Appointment> events = new ArrayList<Appointment>();
		for(Appointment appointment: appointmentManager.Appointments) {
			
			// yearly case
			if(appointment.getType() == 0) {
				
				// repeat every year
				if(appointment.getRepeat() == 1) {
					if(appointment.getMonth() == lunar.getMonth()) {							
						if(appointment.getDay() == lunar.getDay()) {
							events.add(appointment);
						}
					
						// specific for eve case
						if(appointment.getDay() == 0) {
							Date tomorrow = new Date(lunar.getSun().getTime()+1000*60*60*24);
							Lunar tomorrowLunar = getLunar(tomorrow);
							if(lunar.getDay() > tomorrowLunar.getDay()) {
								events.add(appointment);
							}
						}
					}
				} else {
					
					// repeat in every x year
					if((lunar.getSunYear()-appointment.getYear()) % appointment.getRepeat() == 0) {
						if(appointment.getMonth() == lunar.getMonth()) {							
							if(appointment.getDay() == lunar.getDay()) {
								events.add(appointment);
							}
						
							// specific for eve case
							if(appointment.getDay() == 0) {
								Date tomorrow = new Date(lunar.getSun().getTime()+1000*60*60*24);
								Lunar tomorrowLunar = getLunar(tomorrow);
								if(lunar.getDay() > tomorrowLunar.getDay()) {
									events.add(appointment);
								}
							}
						}
					}
				}
			}
		}//end loops
		
		return events;
	}

	/**
	 * Compare two date values and return the different in days.
	 * 
	 * @param date1 Last date
	 * @param date2 Previous date
	 * @return Positive integer if date1 bigger than date2 otherwise negative.
	 */
	public int diffDays(Date date1, Date date2) {
		long diffInSecs = (date1.getTime() - date2.getTime()) / 1000;
		return (int) (diffInSecs / 86400);// one day has 86,400 seconds
	}
	
	/**
	 * Return the next new moon value.
	 * @param start The index in data collection start to seek.
	 * @return
	 */
	private Lunar getNextNewMoon(int start) {
		
		if(this.currentNewMoon == null) return null;		
		Lunar lastMoon = this.currentNewMoon;		
		for (int i = start; i < this.data.size(); i++) {
			Lunar newMoon = this.data.get(i);
			if (newMoon.getMonth() > 0) {
				if(newMoon.getMonth() == lastMoon.getMonth())
					newMoon.setLeapMonth();
				return newMoon;
			}
		}

		return new Lunar(1,1,1);
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