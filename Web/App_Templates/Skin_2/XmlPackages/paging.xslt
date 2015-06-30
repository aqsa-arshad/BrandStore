<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:aspdnsf="urn:aspdnsf"
				xmlns:mobile="urn:mobile"
				exclude-result-prefixes="msxsl aspdnsf mobile ">
	<xsl:output method="html" indent="yes"/>

    <xsl:param name="currentEntity" select="/root/EntityHelpers/*[name()=/root/Runtime/EntityName]/descendant::Entity[EntityID=/root/Runtime/EntityID]/Entity" />

    <xsl:param name="BaseURL">
  <xsl:choose>
      <xsl:when test="$currentEntity">
          <xsl:value-of select="aspdnsf:EntityLink(/root/Runtime/EntityID, $currentEntity/SEName, /root/Runtime/EntityName, 0)" />
      </xsl:when>
  </xsl:choose>
  </xsl:param>
	<xsl:param name="PageNumURL">
		<xsl:choose>
			<xsl:when test="/root/QueryString/searchterm">
				<xsl:value-of select="concat($BaseURL, '?searchterm=', /root/QueryString/searchterm, '&amp;pagenum=')" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="concat($BaseURL, '?pagenum=')" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:param>
	<xsl:param name="CurrentPage">
		<xsl:choose>
			<xsl:when test="/root/QueryString/pagenum">
				<xsl:value-of select="/root/QueryString/pagenum" />
			</xsl:when>
			<xsl:otherwise>1</xsl:otherwise>
		</xsl:choose>
	</xsl:param>

	<xsl:param name="NumPages">
		<xsl:choose>
			<xsl:when test="/root/Products2/Product/pages">
				<xsl:value-of select="/root/Products2/Product/pages" />
			</xsl:when>
			<xsl:otherwise>1</xsl:otherwise>
		</xsl:choose>
	</xsl:param>


	<xsl:param name="PreviousPage">
		<xsl:choose>
			<xsl:when test="$CurrentPage > 1">
				<xsl:value-of select="$CurrentPage - 1" />
			</xsl:when>
			<xsl:otherwise>1</xsl:otherwise>
		</xsl:choose>
	</xsl:param>

	<xsl:param name="NextPage">
		<xsl:choose>
			<xsl:when test="$CurrentPage &lt; $NumPages">
				<xsl:value-of select="$CurrentPage + 1" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$NumPages" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:param>

	<xsl:param name="NextPageURL">
		<xsl:value-of select="concat($PageNumURL, $NextPage)" />
	</xsl:param>

	<xsl:param name="PreviousPageURL">
		<xsl:value-of select="concat($PageNumURL, $PreviousPage)" />
	</xsl:param>

	<xsl:template name="Paging">
		<xsl:if test="$NumPages > 1">
			<div class="productListPaging">
			<xsl:if test="$CurrentPage > 1">
				<div class="productListPagingContent floatLeft">
          <a href="{$PreviousPageURL}" data-role="button" data-icon="arrow-l">
            <xsl:value-of select="aspdnsf:StringResource('Mobile.Paging.Previous')" disable-output-escaping="yes" />
          </a>
				</div>
			</xsl:if>

				<div id="CurrentPage" style="float:left;margin-left:5px;text-align:center;font-weight:bold;">
					<xsl:text>Page</xsl:text>
					
					<xsl:value-of select="$CurrentPage"/>
					<xsl:text>&#160;of&#160;</xsl:text>
					<xsl:value-of select="$NumPages"/>
				</div>

			<xsl:if test="$CurrentPage &lt; $NumPages">
				<div class="productListPagingContent floatRight">
					<a href="{$NextPageURL}" data-role="button" data-icon="arrow-r" data-iconpos="right">
						<xsl:value-of select="aspdnsf:StringResource('Mobile.Paging.Next')" disable-output-escaping="yes" />
					</a>
				</div>
			</xsl:if>
			</div>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>
