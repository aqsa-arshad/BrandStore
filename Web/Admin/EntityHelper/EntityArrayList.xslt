<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" standalone="yes"/>

  <xsl:param name="ForParentEntityID"></xsl:param>
  <xsl:param name="filterID"></xsl:param>
  <xsl:param name="custlocale"></xsl:param>
  <xsl:param name="deflocale"></xsl:param>
  <xsl:param name="disableEscaping"></xsl:param>

  <xsl:template match="root">

    <xsl:choose>
      <xsl:when test="$ForParentEntityID=0">
        <xsl:apply-templates select="Entity[./EntityID!=$filterID]">
          <xsl:with-param name="prefix"></xsl:with-param>
        </xsl:apply-templates>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="descendant-or-self::Entity[EntityID=$ForParentEntityID]" >
        </xsl:apply-templates>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template match="Entity">
    <xsl:param name="prefix"></xsl:param>
    <xsl:param name="eName">
      <xsl:choose>
        <xsl:when test="count(Name/ml/locale[@name=$custlocale])!=0">
          <xsl:value-of select="Name/ml/locale[@name=$custlocale]"/>
        </xsl:when>
        <xsl:when test="count(Name/ml/locale[@name=$deflocale])!=0">
          <xsl:value-of select="Name/ml/locale[@name=$deflocale]"/>
        </xsl:when>
        <xsl:when test="count(Name/ml)=0">
          <xsl:value-of select="Name"/>
        </xsl:when>
      </xsl:choose>
    </xsl:param>

    <xsl:value-of select="EntityID"/>
    <xsl:text>|</xsl:text>
    <xsl:choose>
      <xsl:when test="$disableEscaping = 'True'">
        <xsl:value-of select="concat($prefix, $eName)" disable-output-escaping="yes"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat($prefix, $eName)"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>~</xsl:text>
    <xsl:apply-templates select="Entity[./EntityID!=$filterID]">
      <xsl:with-param name="prefix" select="concat($prefix, $eName, ' -&gt; ')"></xsl:with-param>
    </xsl:apply-templates>
  </xsl:template>
</xsl:stylesheet>