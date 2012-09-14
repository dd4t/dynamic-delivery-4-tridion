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

  <xsl:template match="embeddedSchema" />

    <xsl:template match="fieldSet">
    <xsl:copy>
      <fields>
        <xsl:apply-templates/>
      </fields>
      <schema>
        <xsl:apply-templates select="../../embeddedSchema/*"/>
      </schema>
    </xsl:copy>
  </xsl:template>

  <!-- This should probably be fixed in the .net templates instead. -->
  <xsl:template match="dateTimeValues/datetime">
    <date>
      <xsl:apply-templates/>
    </date>
  </xsl:template>

  <xsl:template match="field/values">
    <textValues>
      <xsl:apply-templates/>
    </textValues>
  </xsl:template>

  <xsl:template match="metadataFields">
    <metadata>
      <xsl:apply-templates/>
    </metadata>
  </xsl:template>

  <xsl:template match="filename">
    <fileName>
      <xsl:apply-templates/>
    </fileName>
  </xsl:template>

  <xsl:template match="rootElementName">
    <rootElement>
      <xsl:apply-templates/>
    </rootElement>
  </xsl:template>

</xsl:stylesheet>

