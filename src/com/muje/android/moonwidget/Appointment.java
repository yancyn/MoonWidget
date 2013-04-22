package com.muje.android.moonwidget;

import java.util.ArrayList;
import java.util.Date;

import android.util.Log;

public class Appointment {
	/** Repeat every lunar year. **/
	final int YEARLY = 0;
	/** Repeat in every lunar month. Leap month will be ignore
	 * and if repeatence day value fall in leap month,
	 * then the day before will be used.
	 */
	final int MONTHLY = 1;
	/** Recurrence in period of days. **/
	final int DAY = 2;
	
	private int year;
	private int month;
	private int day;
	private String description;
	private int repeat;
	/**
	 * Indicate recurrence type.
	 */
	private int type;
	
	public int getYear() {
		return this.year;
	}
	public int getMonth() {
		return this.month;
	}
	public int getDay() {
		return this.day;
	}
	public String getDescription() {
		return this.description;
	}
	public int getRepeat() {
		return this.repeat;
	}
	public int getType() {
		return this.type;
	}
	
	public Appointment(int year, int month, int day, String description) {
		this.year = year;
		this.month = month;
		this.day = day;
		this.description = description;
		this.repeat = 0;
		this.type = 0;
	}
	public Appointment(int repeat, int type, int year, int month, int day, String description) {
		this.year = year;
		this.month = month;
		this.day = day;
		this.description = description;
		this.repeat = repeat;
		this.type = type;
	}
	/**
	 * Override toString().
	 * TODO: Display recurrence info.
	 */
	public String toString() {
		return String.format("%d-%d-%d %s", year, month, day, description);
	}
}
