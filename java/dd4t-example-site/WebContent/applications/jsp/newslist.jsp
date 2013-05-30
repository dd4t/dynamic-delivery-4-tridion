<%@page import="org.dd4t.springmvc.util.ApplicationContextProvider"%>
<%@page import="org.dd4t.core.resolvers.LinkResolver"%>
<%@page import="java.util.List"%>
<%@page import="org.dd4t.springmvc.apps.listings.NewsList"%>
<%@page import="org.dd4t.contentmodel.GenericComponent"%>
<%@page import="org.dd4t.springmvc.constants.Constants"%>
<%@ page language="java" contentType="text/html; charset=ISO-8859-1"
    pageEncoding="ISO-8859-1"%>

<%
	GenericComponent comp = (GenericComponent) request.getAttribute(Constants.COMPONENT_KEY);

	String title = (String) comp.getContent().get("title").getValues().get(0);

	List<GenericComponent> newsitems = (List<GenericComponent>) request.getAttribute(NewsList.NEWSLIST_COMPS_KEY);
	
	LinkResolver resolver = (LinkResolver) ApplicationContextProvider.getBean("LinkResolver");
%>
<h3><%=title %></h3>

<ul>
<%

	for(GenericComponent newsItem : newsitems){
		String newsTitle = (String) newsItem.getContent().get("title").getValues().get(0);		
		String newsUrl = resolver.resolve(newsItem);
%>		
		<li><a href="<%=newsUrl%>"><%=newsTitle %></a></li>
<%		
	}
%>
</ul>