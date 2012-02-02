package com.muje.android.moonwidget;

import java.util.ArrayList;
import java.util.Date;

public class Lunar {

	/**
	 * TODO: How to implement lunar year. What suppose to display?
	 */
	private int year;
	private int month;
	private int date;
	private Date sun;
	/**
	 * Indicate 24 solar terms.
	 */
	private String term;
	/**
	 * Default contructor for lunar date.
	 * @param year Gregorian year.
	 * @param month Lunar month.
	 * @param date Lunar date.
	 */
	public Lunar(int year, int month, int date) {
		this.year = year % 100;
		this.month = month;
		this.date = date;
		this.term = "";
	}
	public String toString() {		
		String result = "";
		result += getYear() + getMonth() + getDate();
		result += getTerm();
		return result;
	}
	/**
	 * Return sun Gregorian in year value.
	 * @return
	 */
	public int getSunYear() {
		return this.year;
	}
	/**
	 * Return sun Gregorian in month value.
	 * @return
	 */
	public int getSunMonth() {
		return this.month;
	}
	/**
	 * Return sun Gregorian in date value.
	 * @return
	 */
	public int getSunDate() {
		return this.date;
	}
	/**
	 * Return Chinese Year naming.
	 * 
	 * @return String {@link} http://zh.wikipedia.org/wiki/%E7%94%B2%E5%AD%90
	 */
	public String getYear() {

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
		// TODO: tune error for Chinese Year
		// if the date value not pass lunar first month mean it still a last
		// year
		return jiazi[year % 60 - 4]+ "年";
	}
	/**
	 * Return Chinese lunar month.
	 * TODO: handle leap month if it is happen in leap year.
	 * @return
	 */
	public String getMonth() {
		String[] months = new String[] { "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二" };
		return months[this.month - 1] + "月";
	}
	/**
	 * Return Chinese lunar date value.
	 * @return
	 */
	public String getDate() {
		String[] days = new String[] { "初一", "初二", "初三", "初四", "初五", "初六",
				"初七", "初八", "初九", "初十", "十一", "十二", "十三", "十四", "十五", "十六",
				"十七", "十八", "十九", "廿日", "廿一", "廿二", "廿三", "廿四", "廿五", "廿六",
				"廿七", "廿八", "廿九", "卅日", "卅一" };
		return days[this.date - 1];
	}
	/**
	 * Get the corresponding image value for display.
	 * 
	 * @return
	 */
	public int getImageId() {

		Integer[] images = new Integer[] { R.drawable.m00, R.drawable.m01,
				R.drawable.m02, R.drawable.m03, R.drawable.m04, R.drawable.m05,
				R.drawable.m06, R.drawable.m07, R.drawable.m08, R.drawable.m09,
				R.drawable.m10, R.drawable.m11, R.drawable.m12, R.drawable.m13,
				R.drawable.m14, R.drawable.m15, R.drawable.m16, R.drawable.m17,
				R.drawable.m18, R.drawable.m19, R.drawable.m20, R.drawable.m21,
				R.drawable.m22, R.drawable.m23, R.drawable.m24, R.drawable.m25,
				R.drawable.m26, R.drawable.m27 };
		
		//handle exception: lunar date suppose don't have 29 or 30
		if (this.date > images.length) {
			return images[0];
		} else {
			return images[this.date - 1];
		}
	}
	/**
	 * Set the corresponding Gregorian date value.
	 * @param sun Date
	 */
	public void setSun(Date sun) {
		this.sun = sun;
	}
	/**
	 * Get the corresponding Gregorian date value.
	 * @return Date
	 */
	public Date getSun() {
		return this.sun;
	}
	/**
	 * Set the 24 solar term value.
	 * @param term
	 */
	public void setTerm(String term) {
		this.term = term;
	}
	/**
	 * Return today solar term if has.
	 * @return
	 */
	public String getTerm() {
		return this.term;
	}
}