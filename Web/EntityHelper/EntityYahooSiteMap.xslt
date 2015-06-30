<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="text" standalone="yes"/>

	<xsl:param name="entity">category</xsl:param>
	<xsl:param name="ForParentEntityID">0</xsl:param>
	<xsl:param name="StoreLoc">http://dotnetstorefront/version57g/</xsl:param>

	<xsl:template match="root">
        <xsl:choose>
        	<xsl:when test="$ForParentEntityID=0"><xsl:apply-templates select="Entity" /></xsl:when>
        	<xsl:otherwise><xsl:apply-templates select="descendant-or-self::Entity[EntityID=$ForParentEntityID]" /></xsl:otherwise>
        </xsl:choose>
		
	</xsl:template>

	<xsl:template match="Entity">
		<xsl:value-of select="concat($StoreLoc, 'yahooentity.aspx?entityname=', $entity, '&amp;entityid=', EntityID, '&#0013;&#0010;')"/>
		<xsl:apply-templates select="Entity" />
	</xsl:template>

</xsl:stylesheet>