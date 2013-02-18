<%@page import="com.capgemini.tridion.cde.siteedit.SiteEditService"%>
<%@page import="com.capgemini.util.spring.ApplicationContextProvider"%>
<%@page import="org.dd4t.core.resolvers.LinkResolver"%>
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
	<h3>
		<%=SiteEditService.generateSiteEditFieldMarking(comp.getContent().get("title")) %>		
		<%=comp.getContent().get("title").getValues().get(0) %>
	</h3>
	
	<p><strong>
		<%=SiteEditService.generateSiteEditFieldMarking(comp.getContent().get("title")) %>
		<%=comp.getContent().get("introduction").getValues().get(0) %>
	</strong></p>
	
		<%
		LinkResolver resolver = (LinkResolver) ApplicationContextProvider.getBean("LinkResolver");
		String url = resolver.resolve(comp);
	%>
	
	<a href="<%=url %>">Read more</a>