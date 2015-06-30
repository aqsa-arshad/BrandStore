<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ror="http://rorweb.com/0.1/">
    <xsl:output method="html" standalone="yes" omit-xml-declaration="yes" />

	<xsl:param name="entity"></xsl:param>
	<xsl:param name="ForParentEntityID"></xsl:param>
	<xsl:param name="StoreLoc"></xsl:param>

	<xsl:template match="root">
        <xsl:choose>
        	<xsl:when test="$ForParentEntityID=0"><xsl:apply-templates select="Entity" /></xsl:when>
        	<xsl:otherwise><xsl:apply-templates select="descendant-or-self::Entity[EntityID=$ForParentEntityID]" /></xsl:otherwise>
        </xsl:choose>
		
	</xsl:template>

	<xsl:template match="Entity">
	   <item>
	      <title>Products</title>
	      <ror:type>Product</ror:type>
			<ror:seeAlso>
				<xsl:value-of select="$StoreLoc"/>rorentity.aspx?entityname=<xsl:value-of select="$entity"/>&amp;entityid=<xsl:value-of select="EntityID"/>
			</ror:seeAlso>
	   </item>
	   <xsl:apply-templates select="Entity" />
	</xsl:template>

</xsl:stylesheet>