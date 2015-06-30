<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:aspdnsf="urn:aspdnsf"
				xmlns:mobile="urn:mobile"
				exclude-result-prefixes="msxsl aspdnsf mobile ">
	<xsl:output method="html" indent="yes"/>

	<!-- Values to override -->
	
	<xsl:param name="PageCount">
    <xsl:choose>
      <xsl:when test="/root/Products2/Product/pages">
        <xsl:value-of select="/root/Products2/Product/pages" />
      </xsl:when>
      <xsl:otherwise>0</xsl:otherwise>
    </xsl:choose>
  </xsl:param>
	
  <xsl:param name="ProductCount" >
    <xsl:choose>
      <xsl:when test="/root/Products2/Product/ProductCount">
        <xsl:value-of select="/root/Products2/Product/ProductCount" />
      </xsl:when>
      <xsl:otherwise>999</xsl:otherwise>
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

	<xsl:param name="PageSize">
		<xsl:choose>
			<xsl:when test="/root/QueryString/pagesize">
				<xsl:value-of select="/root/QueryString/pagesize" />
			</xsl:when>
			<xsl:otherwise>8</xsl:otherwise>
		</xsl:choose>
	</xsl:param>

	<xsl:param name="PageSorting">
		<xsl:choose>
			<xsl:when test="/root/QueryString/sortby">
				<xsl:value-of select="/root/QueryString/sortby" />
			</xsl:when>
			<xsl:otherwise>relevance</xsl:otherwise>
		</xsl:choose>
	</xsl:param>
		
	<xsl:param name="DisplayDescriptivePageNumber" select="true()" />
	<xsl:param name="DisplayPagePrompt" select="true()" />
	<xsl:param name="DisplayFirstAndLastPageButtons" select="true()" />
	<xsl:param name="DisplayNextAndPreviousPageButtons" select="true()" />
	<xsl:param name="FirstPageButtonContent">&lt;&lt;</xsl:param>
	<xsl:param name="LastPageButtonContent">&gt;&gt;</xsl:param>
	<xsl:param name="PreviousPageButtonContent">&lt;</xsl:param>
	<xsl:param name="NextPageButtonContent">&gt;</xsl:param>

	<!-- End values to override -->

	<xsl:param name="PagesToShow" select="5" />
  
	<xsl:param name="PagingRemainder" select="$CurrentPage mod $PagesToShow" />
  
	<xsl:param name="BackwardPages">
    <xsl:choose>
      <xsl:when test="$PagingRemainder = 0">
        <xsl:value-of select="$PagesToShow - 1" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$PagingRemainder - 1" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:param>
  
	<xsl:param name="ForwardPages">
    <xsl:choose>
      <xsl:when test="$PagingRemainder = 0">
        <xsl:value-of select="0" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$PagesToShow - $PagingRemainder" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:param>
  
  <xsl:template name="ProductControl">
    <xsl:param name="uniqueID" />
    <div class="productControl">
      <div class="entityPageSortWrap">
        <xsl:call-template name="sortby">
          <xsl:with-param name="uniqueID" select="$uniqueID" />
        </xsl:call-template>
        <xsl:call-template name="ProductsPerPage">
          <xsl:with-param name="uniqueID" select="$uniqueID" />
        </xsl:call-template>
		<div class="clearable"></div>
      </div>
      <div class="entityPagePagingWrap">
        <xsl:call-template name="paging" />
      </div>
      <div style="clear:both;"></div>
    </div>
  </xsl:template>

  <xsl:template name="sortby">
    <xsl:param name="uniqueID" />

		<div class="catSortBy" id="catSortBy{$uniqueID}">
      <span class="sortbylabel productcontrollabel">
		<xsl:value-of select="aspdnsf:StringResource('GuidedNavigation.SortBy')" disable-output-escaping="yes" />
        <xsl:text>&#32;</xsl:text>
      </span>
    <select id="SelectSort{$uniqueID}" onchange="setParam('sortby', this.value)" name="SelectSort">
      <option value="relevance">
        <xsl:if test="$PageSorting = 'relevance' or $PageSorting = 'default' or not($PageSorting)">
          <xsl:attribute name="selected">selected</xsl:attribute>
        </xsl:if>
        <xsl:value-of select="aspdnsf:StringResource('GuidedNavigation.SortByRelevance')" disable-output-escaping="yes" />
      </option>
      <option value="priceasc">
        <xsl:if test="$PageSorting = 'priceasc'">
          <xsl:attribute name="selected">selected</xsl:attribute>
        </xsl:if>
		<xsl:value-of select="aspdnsf:StringResource('GuidedNavigation.SortByLowPrice')" disable-output-escaping="yes" />
      </option>
      <option value="pricedesc">
        <xsl:if test="$PageSorting = 'pricedesc'">
          <xsl:attribute name="selected">selected</xsl:attribute>
        </xsl:if>
		<xsl:value-of select="aspdnsf:StringResource('GuidedNavigation.SortByHighPrice')" disable-output-escaping="yes" />
      </option>
      <option value="name">
        <xsl:if test="$PageSorting = 'name'">
          <xsl:attribute name="selected">selected</xsl:attribute>
        </xsl:if>
        <xsl:value-of select="aspdnsf:StringResource('GuidedNavigation.SortByName')" disable-output-escaping="yes" />
      </option> 
    </select>
    </div>
  </xsl:template>

<xsl:template name="paging">    
	<div class="pagerWrap">
		<xsl:if test="$DisplayDescriptivePageNumber = true()">
			<div class="viewingPageHeading productcontrollabel">
				<xsl:variable name="PageDesriptionLabelReplace" select="aspdnsf:StrReplace(aspdnsf:StringResource('GuidedNavigation.PageLocationDescription'), '{0}', $CurrentPage)" />
				<xsl:value-of select="aspdnsf:StrReplace($PageDesriptionLabelReplace, '{1}', $PageCount)" disable-output-escaping="yes" />
			</div>
		</xsl:if>
		<xsl:if test="$PageCount &gt; 1">
			<div class="pageLinksWrap">
				<xsl:if test="$DisplayPagePrompt = true()">
					<span class="pageHeadingWrap">
						<xsl:value-of select="aspdnsf:StringResource('GuidedNavigation.PageLabel')" disable-output-escaping="yes" />
					</span>
				</xsl:if>
				<xsl:if test="$CurrentPage &gt; 1 and ($DisplayFirstAndLastPageButtons = true() or $DisplayNextAndPreviousPageButtons = true())">
					<span class="pageArrowWrap">
						<xsl:if test="$DisplayFirstAndLastPageButtons = true()">
							<a class="pagelink firstLink" href="javascript:void(0);" onclick="setParam('pagenum', '{1}');">
								<xsl:value-of select="$FirstPageButtonContent" />
							</a>
						</xsl:if>
						<xsl:if test="$DisplayNextAndPreviousPageButtons = true()">
							<a class="pagelink prevLink" href="javascript:void(0);" onclick="setParam('pagenum', '{$CurrentPage - 1}');">
								<xsl:value-of select="$PreviousPageButtonContent" />
							</a>
						</xsl:if>
					</span>
				</xsl:if>
				<xsl:call-template name="pagelink">
				  <xsl:with-param name="page">
					<xsl:value-of select="$CurrentPage - $BackwardPages" />
				  </xsl:with-param>
				</xsl:call-template>
				<xsl:if test="$CurrentPage &lt; $PageCount and ($DisplayFirstAndLastPageButtons = true() or $DisplayNextAndPreviousPageButtons = true())">
					<span class="pageArrowWrap">
						<xsl:if test="$DisplayNextAndPreviousPageButtons = true()">
							<a class="pagelink nextLink" href="javascript:void(0);" onclick="setParam('pagenum', '{$CurrentPage + 1}');">
								<xsl:value-of select="$NextPageButtonContent" />
							</a>
						</xsl:if>
						<xsl:if test="$DisplayFirstAndLastPageButtons = true()">
							<a class="pagelink lastLink" href="javascript:void(0);" onclick="setParam('pagenum', '{$PageCount}');">
								<xsl:value-of select="$LastPageButtonContent" />
							</a>
						</xsl:if>
					</span>
				</xsl:if>
			</div>
		</xsl:if>
		<div class="clearable"></div>
	</div>    
</xsl:template>

  <xsl:template name="pagelink">
    <xsl:param name="page" />
    <xsl:if test="$page &gt; 0 and $page &lt;= $CurrentPage + $ForwardPages">
      <xsl:choose>
        <xsl:when test="$page = $CurrentPage">
          <a class="currentpage pagelink">
            <xsl:value-of select="$page" />
          </a>
        </xsl:when>
        <xsl:otherwise>
          <a href="javascript:void(0);" class="pagelink" onclick="setParam('pagenum', '{$page}');">
            <xsl:value-of select="$page" />
          </a>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
    <xsl:if test="$page &lt; $CurrentPage + $ForwardPages and $page &lt; $PageCount">
      <xsl:call-template name="pagelink">
        <xsl:with-param name="page">
          <xsl:value-of select="$page + 1" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="ProductsPerPage">
    <xsl:param name="uniqueID" />
      <xsl:if test="$ProductCount > 0">
        <div class="pagesize">
        <span class="pageSizeHeading productcontrollabel">
			<xsl:value-of select="aspdnsf:StringResource('GuidedNavigation.PageSizeLabel')" disable-output-escaping="yes" />
			<xsl:text>&#32;</xsl:text>
		</span>

          <select id="PageSize{$uniqueID}" onchange="setParam('pagesize', this.value)" name="PageSize" >

            <option value="12">
              <xsl:if test="$PageSize = 12 or $PageSize = ''">
                <xsl:attribute name="selected">selected</xsl:attribute>
              </xsl:if>
              <xsl:text>12</xsl:text>
            </option>
            <option value="24">
              <xsl:if test="$PageSize = 24">
                <xsl:attribute name="selected">selected</xsl:attribute>
              </xsl:if>
              <xsl:text>24</xsl:text>
            </option>
            <option value="48">
              <xsl:if test="$PageSize = 48">
                <xsl:attribute name="selected">selected</xsl:attribute>
              </xsl:if>
              <xsl:text>48</xsl:text>
            </option>
            <option value="100">
            <xsl:if test="$PageSize = 100">
                <xsl:attribute name="selected">selected</xsl:attribute>
              </xsl:if>
            <xsl:choose>
              <xsl:when test="$ProductCount > 100">
                <xsl:text>100</xsl:text>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="aspdnsf:StringResource('GuidedNavigation.PageSizeViewAll')" disable-output-escaping="yes" />
              </xsl:otherwise>
            </xsl:choose>
            </option>
          </select>
        </div>
      </xsl:if>
  </xsl:template>

</xsl:stylesheet>
