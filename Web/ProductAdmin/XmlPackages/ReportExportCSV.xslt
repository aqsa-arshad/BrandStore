<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="text" omit-xml-declaration="yes" indent="no" standalone="yes"/>

    <xsl:template match="root">
		<xsl:apply-templates select="table/tr"/>
	</xsl:template>

	<xsl:template match="tr">
		<xsl:apply-templates select="td" />
		<xsl:text>&#13;&#10;</xsl:text>
	</xsl:template>

	<xsl:template match="td" >
    <xsl:if test="position() != 1"><xsl:text>,</xsl:text></xsl:if>
    <xsl:choose>
      <xsl:when test="contains(., ',') or contains(., '&quot;') or contains(., '&#10;') or contains(., '&#13;')">
				<xsl:text>&quot;</xsl:text>
				<xsl:call-template name="replace">
					<xsl:with-param name="source" select="." />
					<xsl:with-param name="oldValue">
						<xsl:text>&quot;</xsl:text>
					</xsl:with-param>
					<xsl:with-param name="newValue">
						<xsl:text>&quot;&quot;</xsl:text>
					</xsl:with-param>
				</xsl:call-template>
				<xsl:text>&quot;</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="." disable-output-escaping="yes" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="replace">
		<xsl:param name="source" />
		<xsl:param name="oldValue" />
		<xsl:param name="newValue" />
		<xsl:choose>
			<xsl:when test="contains($source, $oldValue)">
				<xsl:value-of select="substring-before($source, $oldValue)" />
				<xsl:value-of select="$newValue" />
				<xsl:call-template name="replace">
					<xsl:with-param name="source" select="substring-after($source, $oldValue)" />
					<xsl:with-param name="oldValue" select="$oldValue" />
					<xsl:with-param name="newValue" select="$newValue" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$source" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>