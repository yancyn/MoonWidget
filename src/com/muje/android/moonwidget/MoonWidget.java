package com.muje.android.moonwidget;

import java.io.IOException;
import java.util.Date;

import org.xmlpull.v1.XmlPullParserException;

import android.appwidget.AppWidgetManager;
import android.appwidget.AppWidgetProvider;
import android.content.Context;
import android.widget.RemoteViews;

public class MoonWidget extends AppWidgetProvider {

	private RemoteViews remoteViews;

	@Override
	public void onUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds) {

		super.onUpdate(context, appWidgetManager, appWidgetIds);
		this.remoteViews = new RemoteViews(context.getPackageName(), R.layout.main);
		LunarCalendar calendar = new LunarCalendar();

		try {
			calendar.initialize(context);
			Lunar lunar = calendar.getLunar(new Date());

			String text = lunar.getYearText();
			text += "\n" + lunar.getMonthText() + lunar.getDayText();
			if (lunar.getTerm().length() > 0)
				text += "\n" + lunar.getTerm();
			this.remoteViews.setTextViewText(R.id.todayText, text);
			this.remoteViews.setImageViewResource(R.id.moonImage, lunar.getImageId());
			// this.remoteViews.setTextViewText(R.id.todayRemark,"");//calendar.getToday().getYear());;
			// this.remoteViews.setTextViewText(R.id.nextMoon,"");

			// this must call ensure the widget take the latest changes
			appWidgetManager.updateAppWidget(appWidgetIds, this.remoteViews);
		} catch (XmlPullParserException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
}