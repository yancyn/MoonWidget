package com.muje.android.moonwidget;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xmlpull.v1.XmlPullParser;
import org.xmlpull.v1.XmlPullParserException;

import android.content.Context;
import android.content.res.Resources;
import android.content.res.XmlResourceParser;
import android.os.Environment;
import android.util.Log;

public class AppointmentManager {
	
	final String FILE_NAME = Environment.getExternalStorageDirectory() + File.separator + "appointments.xml";
	private File file = null;
	public ArrayList<Appointment> Appointments;
	
	public AppointmentManager() {
		this.Appointments = new ArrayList<Appointment>();
	}
	public void initialize(Context context) {
			
		try {
			// First time build the database if not exist
			file = new File(FILE_NAME);
			if(!file.exists()) {
				extract(context);
				write();
			} else {		
				read();	
			}
		} catch(Exception ex) {
			ex.printStackTrace();
		}
	}
	/**
	 * Clone into memory then write to external storage for first time.
	 * TODO: Sort array or xml.
	 */
	private void extract(Context context) throws XmlPullParserException, IOException {
		
		int year = 0;
		int month = 0;
		int day = 0;
		int repeat = 0;
		int type = 0;
		String desc = "";
		Resources resource = context.getResources();
		XmlResourceParser parser = resource.getXml(R.xml.appointments);
		parser.next();
		int eventType = parser.getEventType();
		while(eventType != XmlPullParser.END_DOCUMENT) {
			if(eventType == XmlPullParser.START_TAG) {
				for(int i=0;i<parser.getAttributeCount();i++) {
					String name = parser.getAttributeName(i);
					if(name.compareTo("year") == 0) {
						year = Integer.parseInt(parser.getAttributeValue(i));
					} else if(name.compareTo("month") == 0) {
						month = Integer.parseInt(parser.getAttributeValue(i));
					} else if(name.compareTo("day") == 0) {
						day = Integer.parseInt(parser.getAttributeValue(i));
					} else if(name.compareTo("desc") == 0) {
						desc = parser.getAttributeValue(i);
					} else if(name.compareTo("repeat") == 0) {
						repeat = Integer.parseInt(parser.getAttributeValue(i));
					} else if(name.compareTo("type") == 0) {
						type = Integer.parseInt(parser.getAttributeValue(i));
					}
				}
			}
			
			if(desc.length() > 0) {
				Appointment appointment = new Appointment(repeat, type, year,month,day,desc);
				this.Appointments.add(appointment);
			}
			
			eventType = parser.next();
			// reset
			year = 0;
			month = 0;
			day = 0;
			desc = "";
			repeat = 0;
			type = 0;
		}//end loops		
	}
	/**
	 * Store xml file to local sd card.
	 * @throws Exception
	 */
	private void write() {
		
		Log.d("DEBUG","Attempt to stored...");
		String content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
		content += "<calendar>";
		for(Appointment appointment: this.Appointments) {
			content += String.format("<lunar repeat=\"%d\" type=\"%d\" year=\"%d\" month=\"%d\" day=\"%d\" desc=\"%s\" />",
					appointment.getRepeat(),
					appointment.getType(),
					appointment.getYear(),
					appointment.getMonth(),
					appointment.getDay(),
					appointment.getDescription());
		}//end loops
		content += "</calendar>";
		
		FileWriter writer = null;
		try {
			writer = new FileWriter(file);
			writer.write(content);
			Log.d("DEBUG","appointments.xml is stored successfully.");
		} catch (IOException e) {
			e.printStackTrace();
		} finally {
			if(writer != null) {
				try {
					writer.close();
				} catch (IOException e) {
					e.printStackTrace();
				}
			}
		}
	}
	/**
	 * Read xml from local storage.
	 * @throws Exception
	 */
	private void read() {
		try {
			Document doc = null;
			DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
			DocumentBuilder db = null;
			FileInputStream input = new FileInputStream(FILE_NAME);
			
			try{
				db = factory.newDocumentBuilder();
				doc = db.parse(new InputSource(input));
			} catch(ParserConfigurationException ex) {
				ex.printStackTrace();
			} catch(SAXException ex) {
				ex.printStackTrace();
			}
			
			doc.getDocumentElement().normalize();
			parsing(doc);			
		} catch(Exception ex) {
			ex.printStackTrace();
		}
	}
	/**
	 * Parse local file to appointments collection.
	 * TODO: Sort appointment or document array.
	 * @param document
	 * @throws Exception
	 */
	private void parsing(Document document) throws Exception {
		
		try{
			Log.d("DEBUG","Start parsing from local..");
			this.Appointments.clear();
			NodeList nodes = document.getElementsByTagName("lunar");
			for(int i=0;i<nodes.getLength();i++) {
				
				Node node = nodes.item(i);
				if(node.getNodeType() == Node.ELEMENT_NODE) {
					Element element = (Element)node;
					int year = Integer.parseInt(element.getAttribute("year"));
					int month = Integer.parseInt(element.getAttribute("month"));
					int day = Integer.parseInt(element.getAttribute("day"));
					String desc = element.getAttribute("desc");
					int repeat = Integer.parseInt(element.getAttribute("repeat"));
					int type = Integer.parseInt(element.getAttribute("type"));
				
					Appointment appointment = new Appointment(repeat,type,year,month,day,desc);
					this.Appointments.add(appointment);
				}
			}//end loops
		} catch(Exception ex) {
			ex.printStackTrace();
		}
	}

}
