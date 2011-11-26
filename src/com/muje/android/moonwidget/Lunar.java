package com.muje.android.moonwidget;

public class Lunar {

	/**
	 * TODO: How to implement lunar year. What suppose to display?
	 */
	public int Year;
	public int Month;
	public int Day;
	public String Message;

	public Lunar(int year, int month, int day) {

		this.Year = year;
		// this.Month = month;
		// this.Day = day;
		this.Message = "";
	}

	public String toString() {

		String result = "";
		String[] months = new String[] { "一", "二", "三", "四", "五", "六", "七",
				"八", "九", "十", "十一", "十二" };
		result += months[this.Month - 1] + "月";

		String[] days = new String[] { "初一", "初二", "初三", "初四", "初五", "初六",
				"初七", "初八", "初九", "初十", "十一", "十二", "十三", "十四", "十五", "十六",
				"十七", "十八", "十九", "廿日", "廿一", "廿二", "廿三", "廿四", "廿五", "廿六",
				"廿七", "廿八", "廿九", "卅日", "卅一" };
		result += days[this.Day - 1];
		return result;
	}

	/**
	 * Get the corresponding image value for display.
	 * 
	 * @return
	 */
	public int getImageId() {

		Integer[] images = new Integer[] { R.drawable.m0, R.drawable.m1,
				R.drawable.m2, R.drawable.m3, R.drawable.m4, R.drawable.m5,
				R.drawable.m6, R.drawable.m7, R.drawable.m8, R.drawable.m9,
				R.drawable.m10, R.drawable.m11, R.drawable.m12, R.drawable.m13,
				R.drawable.m14, R.drawable.m15, R.drawable.m16, R.drawable.m17,
				R.drawable.m18, R.drawable.m19, R.drawable.m20, R.drawable.m21,
				R.drawable.m22, R.drawable.m23, R.drawable.m24, R.drawable.m25,
				R.drawable.m26, R.drawable.m27 };
		return images[this.Day - 1];
	}
}
