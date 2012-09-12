<%@ page 
    language="java" contentType="text/html; charset=UTF-8"
	import="com.capgemini.tridion.cde.view.model.*,
		    com.capgemini.tridion.cde.*,
			com.tridion.extensions.dynamicdelivery.foundation.contentmodel.*,
			com.tridion.extensions.dynamicdelivery.foundation.contentmodel.impl.*"
	pageEncoding="UTF-8"%>
	
<%
	GenericComponent comp = (GenericComponent) request.getAttribute("Component");

%>
	<h2><%=comp.getContent().get("title").getValues().get(0) %></h2>
	
	<a href="<%=comp.getResolvedUrl() %>">Read more</a>