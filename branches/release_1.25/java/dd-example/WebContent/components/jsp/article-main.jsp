<%@page import="com.capgemini.tridion.cde.siteedit.SiteEditService"%>
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
	<h2>
		<%=SiteEditService.generateSiteEditFieldMarking("title") %>
		<%=comp.getContent().get("title").getValues().get(0) %>
	</h2>
	
	<p><strong>
		<%=SiteEditService.generateSiteEditFieldMarking("introduction") %>
		<%=comp.getContent().get("introduction").getValues().get(0) %>
	</strong></p>
	
	<div>
		<%=SiteEditService.generateSiteEditFieldMarking("paragraph") %>
		<%=comp.getContent().get("paragraph").getValues().get(0) %>
	</div>
	
	