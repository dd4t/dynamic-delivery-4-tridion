<%@page import="org.dd4t.core.resolvers.LinkResolver"%>
<%@page import="com.capgemini.util.spring.ApplicationContextProvider"%>
<%@ page 
    language="java" contentType="text/html; charset=UTF-8"
	import="com.capgemini.tridion.cde.view.model.*,
		    com.capgemini.tridion.cde.*,
			org.dd4t.contentmodel.*,
			org.dd4t.contentmodel.impl.*"
	pageEncoding="UTF-8"%>
	
<%
	GenericComponent comp = (GenericComponent) request.getAttribute("Component");

%>
	<h2><%=comp.getContent().get("title").getValues().get(0) %></h2>
		
	<%
		LinkResolver resolver = (LinkResolver) ApplicationContextProvider.getBean("LinkResolver");
		String url = resolver.resolve(comp);
	%>
	<a href="<%=url %>">Read more</a>
	