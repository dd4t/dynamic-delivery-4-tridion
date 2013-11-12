<%@page import="org.dd4t.springmvc.util.ApplicationContextProvider"%>
<%@page import="org.dd4t.core.resolvers.LinkResolver"%>
<%@page import="org.dd4t.springmvc.siteedit.SiteEditService"%>
<%@ page 
    language="java" contentType="text/html; charset=UTF-8"
	import="org.dd4t.springmvc.view.model.*,
			org.dd4t.contentmodel.*,
			org.dd4t.contentmodel.impl.*"
	pageEncoding="UTF-8"%>
	
<%
	GenericComponent comp = (GenericComponent) request.getAttribute("Component");	 
%>
	<h2>
		<%=SiteEditService.generateSiteEditFieldMarking(comp.getContent().get("title")) %>		
		<%=comp.getContent().get("title").getValues().get(0) %>
	</h2>
	
	<div>
		<%=SiteEditService.generateSiteEditFieldMarking(comp.getContent().get("paragraph")) %>
	
		<%=comp.getContent().get("paragraph").getValues().get(0) %>
	</div>
	
	<h3>Related links</h3>
	<ul>
			<%=SiteEditService.generateSiteEditFieldMarking(comp.getContent().get("related")) %>
	
	<%
	for(Object ob : comp.getContent().get("related").getValues()){	    
	    GenericComponent linkedComp = (GenericComponent) ob;		
	
		// since the news main template is setup with linklevels == 2, the linked component content is loaded inside
				
		String title = (String) linkedComp.getContent().get("title").getValues().get(0);		
		LinkResolver linkResolver = (LinkResolver) ApplicationContextProvider.getBean("LinkResolver");
		String href = linkResolver.resolve(linkedComp);
		
		%>
		<li><a href="<%=href %>"><%=title %></a></li>	
		<%
	} 
	%>
	</ul>
	
	
	