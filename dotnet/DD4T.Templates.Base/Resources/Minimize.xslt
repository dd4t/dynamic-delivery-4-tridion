<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <!-- 
      MINIMIZE.XSLT
      
      Added in 1.26 to enable a minimization of the output, tuned for the Java variant. By ensuring
      no empty nodes in the resultset, and not having indentation, the published size is as small
      as it can be which lowers storage requirements and improves deserialization results.
      
      Rogier Oudshoorn, 24 november 2012
    -->

  <xsl:param name="remove-publications" select="false()"/>
  <xsl:param name="remove-folders" select="false()"/>

  <!-- no indentation means smaller result string -->
    <xsl:output method="xml" indent="no"/>

    <!-- template to match all -->
    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
    </xsl:template>

  <!-- more specific template to match empty nodes, removes them from resultset -->
  <xsl:template match="*[. = '']"/>
  
  <!-- remove all publication and folder nodes except the one on the highest level -->
  <xsl:template match="publication[count(ancestor::*)&gt;1]|Publication[count(ancestor::*)&gt;1]|folder[count(ancestor::*)&gt;1]|Folder[count(ancestor::*)&gt;1]">
    <xsl:if test="not($remove-publications)">
      <xsl:copy>
        <xsl:apply-templates select="@* | node()"/>
      </xsl:copy>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
