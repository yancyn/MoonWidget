package com.muje.android.moonwidget;

import java.io.IOException;
import java.util.Date;

import org.xmlpull.v1.XmlPullParserException;

import android.app.PendingIntent;
import android.appwidget.AppWidgetManager;
import android.appwidget.AppWidgetProvider;
import android.content.Context;
import android.content.Intent;
import android.widget.RemoteViews;

public class MoonWidget extends AppWidgetProvider {

	private static String LAUNCH = "LAUNCH";

	@Override
	public void onUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds) {

		super.onUpdate(context, appWidgetManager, appWidgetIds);
		RemoteViews remoteViews = new RemoteViews(context.getPackageName(), R.layout.main);

		// add onClick for the Moon
		Intent i = new Intent(context, MoonWidget.class);
		i.setAction(LAUNCH);
		PendingIntent pi = PendingIntent.getBroadcast(context, 0, i, 0);
		remoteViews.setOnClickPendingIntent(R.id.moonImage, pi);

		LunarCalendar calendar = new LunarCalendar();

		try {
			calendar.initialize(context);
			Date now = new Date();
			Date dateOnly = new Date(now.getYear(),now.getMonth(),now.getDate());
			Lunar lunar = calendar.getLunar(dateOnly);

			String text = lunar.getYearText();
			text += "\n" + lunar.getMonthText() + lunar.getDayText();
			if (lunar.getTerm().length() > 0)
				text += "\n" + lunar.getTerm();
			remoteViews.setTextViewText(R.id.todayText, text);
			remoteViews.setImageViewResource(R.id.moonImage, lunar.getImageId());
			// this.remoteViews.setTextViewText(R.id.todayRemark,"");//calendar.getToday().getYear());;
			// this.remoteViews.setTextViewText(R.id.nextMoon,"");

			// this must call ensure the widget take the latest changes
		} catch (XmlPullParserException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		}

		appWidgetManager.updateAppWidget(appWidgetIds, remoteViews);
	}

	@Override
	public void onReceive(Context context, Intent intent) {

		super.onReceive(context, intent);
		if(intent.getAction().equals(LAUNCH)) {
			Intent i = new Intent(Intent.ACTION_VIEW);
			i.setClassName("com.muje.android.moonwidget", "com.muje.android.moonwidget.CalendarActivity");
			i.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
			context.startActivity(i);
		}
	}
}