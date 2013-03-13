<%@page import="com.capgemini.tridion.cde.constants.Constants"%>
<%@page import="com.capgemini.tridion.cde.siteedit.SiteEditService"%>
<%@ page 
    language="java" contentType="text/html; charset=UTF-8"
	import="com.capgemini.tridion.cde.view.model.*,
		    com.capgemini.tridion.cde.*,
			org.dd4t.contentmodel.*,
			org.dd4t.contentmodel.impl.*"
	pageEncoding="UTF-8"%>
		
<%
	GenericComponent comp = (GenericComponent) request.getAttribute(Constants.COMPONENT_KEY);
%>
	<h2>
		<%=SiteEditService.generateSiteEditFieldMarking(comp.getContent().get("title")) %>
		<%=comp.getContent().get("title").getValues().get(0) %>
	</h2>
	
	<p><strong>
		<%=SiteEditService.generateSiteEditFieldMarking(comp.getContent().get("introduction")) %>
		<%=comp.getContent().get("introduction").getValues().get(0) %>
	</strong></p>
	
	<div>
		<%=SiteEditService.generateSiteEditFieldMarking(comp.getContent().get("paragraph")) %>
		<%=comp.getContent().get("paragraph").getValues().get(0) %>
	</div>
	
	