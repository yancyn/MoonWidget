package com.muje.android.moonwidget;

import java.util.ArrayList;
import java.util.Date;

public class Lunar {

	private int year;
	private int month;
	private int day;
	private Date sun;
	private boolean isLeapMonth;

	// there is no 31st day in a lunar month
	final static String[] DAYS = new String[] { "初一", "初二", "初三", "初四", "初五", "初六",
			"初七", "初八", "初九", "初十", "十一", "十二", "十三", "十四", "十五", "十六", "十七",
			"十八", "十九", "廿日", "廿一", "廿二", "廿三", "廿四", "廿五", "廿六", "廿七", "廿八",
			"廿九", "卅日" };

	/**
	 * Indicate 24 solar terms.
	 */
	private String term;

	/**
	 * Default constructor for lunar date.
	 * 
	 * @param year
	 *            Gregorian year.
	 * @param month
	 *            Lunar month.
	 * @param day
	 *            Lunar day.
	 */
	public Lunar(int year, int month, int day) {
		this.year = year;// % 100;
		this.month = month;
		this.day = day;
		this.term = "";
	}

	/**
	 * Constructor for setting up solar term only without lunar month value.
	 * 
	 * @param year
	 * @param term
	 */
	public Lunar(int year, String term) {
		this.year = year;// % 100;
		this.month = 0;
		this.day = 0;
		this.term = term;
	}

	/**
	 * Override toString() output.
	 */
	public String toString() {
		String result = "";
		result += getYearText() + getMonthText() + getDayText();
		result += getTerm();
		return result;
	}

	/**
	 * Return sun Gregorian in year value.
	 * 
	 * @return
	 */
	public int getSunYear() {
		return this.year;
	}

	/**
	 * Return lunar month value.
	 * 
	 * @return
	 */
	public int getMonth() {
		return this.month;
	}

	/**
	 * Return lunar day value.
	 * 
	 * @return
	 */
	public int getDay() {
		return this.day;
	}

	/**
	 * Return Chinese Year naming.
	 * 
	 * @return String
	 * @see http://zh.wikipedia.org/wiki/%E7%94%B2%E5%AD%90
	 */
	public String getYearText() {

		// Dictionary jiazi = new Dictionary<>();
		ArrayList<String> temp = new ArrayList<String>();

		// 10 celestial stems
		String[] stems = new String[] { "甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸" };

		// 12 earth branches
		String[] branches = new String[] { "子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥" };

		// computing 60 花甲
		for (int i = 0; i < 60; i++) {
			String label = "";
			label += stems[i % stems.length];// 10
			label += branches[i % branches.length];// 12
			temp.add(label);
		}// end loops

		String[] jiazi = new String[temp.size()];
		jiazi = temp.toArray(jiazi);

		// 1804 is the first 甲子 and so on...
		// TODO: tune buffer for Chinese Year
		// if the date value not pass lunar first month mean it still stick with
		// last year
		return jiazi[(year-4) % 60] + "年";
	}

	/**
	 * Return Chinese lunar month. TODO: handle leap month if it is happen in
	 * leap year.
	 * 
	 * @return
	 */
	public String getMonthText() {
		String output = "月";
		String[] months = new String[] { "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二" };
		if (this.month > 0) {
			output = months[this.month - 1] + output;
			if (this.isLeapMonth)
				output = "润" + output;
		}

		return output;
	}

	/**
	 * Set current month is a leap month.
	 */
	public void setLeapMonth() {
		this.isLeapMonth = true;
	}

	/**
	 * Indicate this month is a leap month.
	 * 
	 * @return
	 */
	public boolean isLeapMonth() {
		return this.isLeapMonth;
	}

	/**
	 * Return Chinese lunar date value.
	 * 
	 * @return
	 */
	public String getDayText() {

		// handle exception case where lunar day suppose not exceed 28
		if (this.day == 0) {
			return DAYS[0];
		} else if (this.day > DAYS.length) {
			return DAYS[0];
		} else {
			return DAYS[this.day - 1];
		}
	}

	/**
	 * Get the corresponding image value for display.
	 * 
	 * @return
	 */
	public int getImageId() {

		// lunar month may contains 29 days or 30 days though
		Integer[] images = new Integer[] { R.drawable.m00, R.drawable.m01,
				R.drawable.m02, R.drawable.m03, R.drawable.m04, R.drawable.m05,
				R.drawable.m06, R.drawable.m07, R.drawable.m08, R.drawable.m09,
				R.drawable.m10, R.drawable.m11, R.drawable.m12, R.drawable.m13,
				R.drawable.m14, R.drawable.m15, R.drawable.m16, R.drawable.m17,
				R.drawable.m18, R.drawable.m19, R.drawable.m20, R.drawable.m21,
				R.drawable.m22, R.drawable.m23, R.drawable.m24, R.drawable.m25,
				R.drawable.m26, R.drawable.m27, R.drawable.m28, R.drawable.m29 };

		// handle exception case where lunar day suppose not exceed 28
		if (this.day == 0) {
			return images[0];
		} else if (this.day > images.length) {
			return images[0];
		} else {
			return images[this.day - 1];
		}
	}

	/**
	 * Set the corresponding Gregorian date value.
	 * 
	 * @param sun
	 *            Date
	 */
	public void setSun(Date sun) {
		this.sun = sun;
	}

	/**
	 * Get the corresponding Gregorian date value.
	 * 
	 * @return Date
	 */
	public Date getSun() {
		return this.sun;
	}

	/**
	 * Set the 24 solar term value.
	 * 
	 * @param term
	 */
	public void setTerm(String term) {
		this.term = term;
	}

	/**
	 * Return today solar term if has. TODO: get solar term by calculation
	 * formula.
	 * 
	 * @return
	 */
	public String getTerm() {
		return this.term;
	}
}