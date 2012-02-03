package com.muje.android.moonwidget;

import java.util.Comparator;

public class LunarComparator implements Comparator<Lunar> {

	@Override
	public int compare(Lunar arg0, Lunar arg1) {
		return arg0.getSun().compareTo(arg1.getSun());
	}
}
