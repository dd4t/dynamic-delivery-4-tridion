<%@page import="org.dd4t.springmvc.siteedit.SiteEditService"%>
<%@page import="org.dd4t.core.resolvers.LinkResolver"%>
<%@page import="org.dd4t.springmvc.util.ApplicationContextProvider"%>
<%@ page 
    language="java" contentType="text/html; charset=UTF-8"
	import="org.dd4t.springmvc.view.model.*,
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
		
	<%
		LinkResolver resolver = (LinkResolver) ApplicationContextProvider.getBean("LinkResolver");
		String url = resolver.resolve(comp);
	%>
	<a href="<%=url %>">Read more</a>
	