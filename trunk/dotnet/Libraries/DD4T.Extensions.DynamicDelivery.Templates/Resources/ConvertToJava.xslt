<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes"/>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
    </xsl:template>
    
    <!-- The elements which are interfaces or abstract classes needs to have the class attribute so the deserializer knows which class to instantiate -->
  <!-- TODO: make this configurable in a parameter schema! -->

			 
  <!-- Remove all key and value elements in metadata, metadatafields because they are not needed and just added because it is a hashmap -->
	<xsl:template match="item/key">
		<xsl:apply-templates/>
	</xsl:template>
	
	<xsl:template match="item/value">
		<xsl:apply-templates/>
	</xsl:template>
	
	<xsl:template match="embeddedValues/fields">
		<xsl:apply-templates/>
	</xsl:template>
	
	<!-- Add class attribute to the field element -->
  <!--
    <xsl:template match="field">
		<xsl:variable name="fieldType"><xsl:value-of select="@fieldType"/></xsl:variable>
		<xsl:choose>
			<xsl:when test="$fieldType='Text'"><xsl:call-template name="outputFieldClass"><xsl:with-param name="className">TextField</xsl:with-param></xsl:call-template></xsl:when>
			<xsl:when test="$fieldType='Keyword'"><xsl:call-template name="outputFieldClass"><xsl:with-param name="className">TextField</xsl:with-param></xsl:call-template></xsl:when>			
			<xsl:when test="$fieldType='Xhtml'"><xsl:call-template name="outputFieldClass"><xsl:with-param name="className">XhtmlField</xsl:with-param></xsl:call-template></xsl:when>			
			<xsl:when test="$fieldType='MultiLineText'"><xsl:call-template name="outputFieldClass"><xsl:with-param name="className">TextField</xsl:with-param></xsl:call-template></xsl:when>			
			<xsl:when test="$fieldType='ExternalLink'"><xsl:call-template name="outputFieldClass"><xsl:with-param name="className">TextField</xsl:with-param></xsl:call-template></xsl:when>			
			<xsl:when test="$fieldType='Embedded'"><xsl:call-template name="outputFieldClass"><xsl:with-param name="className">TextField</xsl:with-param></xsl:call-template></xsl:when>			
			<xsl:when test="$fieldType='Date'"><xsl:call-template name="outputFieldClass"><xsl:with-param name="className">DateField</xsl:with-param></xsl:call-template></xsl:when>
			<xsl:when test="$fieldType='Number'"><xsl:call-template name="outputFieldClass"><xsl:with-param name="className">NumericField</xsl:with-param></xsl:call-template></xsl:when>
			<xsl:when test="$fieldType='ComponentLink'"><xsl:call-template name="outputFieldClass"><xsl:with-param name="className">ComponentLinkField</xsl:with-param></xsl:call-template></xsl:when>
			<xsl:when test="$fieldType='MultiMediaLink'"><xsl:call-template name="outputFieldClass"><xsl:with-param name="className">ComponentLinkField</xsl:with-param></xsl:call-template></xsl:when>
		</xsl:choose>
	</xsl:template>
    
	<xsl:template name="outputFieldClass">
		<xsl:param name="className"/>
    <xsl:param name="fieldType"/>
    <xsl:element name="field">
      <xsl:attribute name="fieldType"><xsl:value-of select="@fieldType"/></xsl:attribute>
      <xsl:apply-templates/>
    </xsl:element>		
	</xsl:template>
-->    
    <!-- This should probably be fixed in the .net templates instead. -->
	<xsl:template match="dateTimeValues/datetime">
		<date><xsl:apply-templates/></date>
	</xsl:template>	
	
	<xsl:template match="field/values">
		<textValues><xsl:apply-templates/></textValues>
	</xsl:template>	

	<xsl:template match="metadataFields">
		<metadata><xsl:apply-templates/></metadata>
	</xsl:template>

  <xsl:template match="filename">
    <fileName>
      <xsl:apply-templates/>
    </fileName>
  </xsl:template>
  
</xsl:stylesheet>
