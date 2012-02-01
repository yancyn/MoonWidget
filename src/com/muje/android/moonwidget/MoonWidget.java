package com.muje.android.moonwidget;

import java.io.IOException;

import org.xmlpull.v1.XmlPullParserException;

import android.appwidget.AppWidgetManager;
import android.appwidget.AppWidgetProvider;
import android.content.Context;
import android.util.Log;
import android.widget.RemoteViews;

public class MoonWidget extends AppWidgetProvider {
	
	private RemoteViews remoteViews;

	@Override
	public void onUpdate(Context context, AppWidgetManager appWidgetManager,
			int[] appWidgetIds) {

		super.onUpdate(context, appWidgetManager, appWidgetIds);		
		this.remoteViews = new RemoteViews(context.getPackageName(), R.layout.main);		
		LunarCalendar calendar = new LunarCalendar();

		try {
			calendar.initialize(context);
			Log.d("DEBUG","Today moon: "+calendar.getToday().toString());
			this.remoteViews.setTextViewText(R.id.todayText, calendar.getToday().toString());
			this.remoteViews.setImageViewResource(R.id.moonImage,calendar.getToday().getImageId());			
			//this.remoteViews.setTextViewText(R.id.todayRemark,"");//calendar.getToday().getYear());;
			//this.remoteViews.setTextViewText(R.id.nextMoon,"");
			
			//this must call ensure the widget take the latest changes
			appWidgetManager.updateAppWidget(appWidgetIds,this.remoteViews);
		} catch (XmlPullParserException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
}