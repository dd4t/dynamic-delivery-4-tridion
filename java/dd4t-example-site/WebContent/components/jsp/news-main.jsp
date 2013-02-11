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
	
	<%=comp.getContent().get("paragraph").getValues().get(0) %>
	
	<h3>Related links</h3>
	<ul>
	<%
	for(Object ob : comp.getContent().get("related").getValues()){	    
	    GenericComponent linkedComp = (GenericComponent) ob;		
	
		// since the news main template is setup with linklevels == 2, the linked component content is loaded inside
				
		String title = (String) linkedComp.getContent().get("title").getValues().get(0);		
		String href = linkedComp.getResolvedUrl();
		
		%>
		<li><a href="<%=href %>"><%=title %></a></li>	
		<%
	} 
	%>
	</ul>
	
	
	