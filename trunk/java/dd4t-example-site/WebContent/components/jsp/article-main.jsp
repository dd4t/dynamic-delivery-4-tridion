<%@page import="org.dd4t.springmvc.constants.Constants"%>
<%@page import="org.dd4t.springmvc.siteedit.SiteEditService"%>
<%@ page 
    language="java" contentType="text/html; charset=UTF-8"
	import="org.dd4t.springmvc.view.model.*,
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
	
	