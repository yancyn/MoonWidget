<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" omit-xml-declaration="yes"/>

  <xsl:template match="/">
    <html>
      <head>
        <title>Reading Book List</title>
        <style type="text/css">
          .header {background:gainsboro;}
          .isbnHead {display:inline-table;width:190px;}
          .isbn {display:inline-table;width:150px;}
          .title {display:inline-table;width:300px;}
          .pageno {display:inline-table;width:60px;}
          .price {display:inline-table;width:60px;}
        </style>
      </head>
      <body>
        <div class="header">
          <span class="isbnHead">ISBN</span>
          <span class="title">Title</span>
          <span class="pageno">Page</span>
          <span class="price">Price</span>
        </div>
        <ol>
          <xsl:for-each select="Books/Book">
            <li>
              <span class="isbn"><xsl:value-of select="ISBN"/></span>
              <span class="title"><xsl:value-of select="Title"/></span>
              <span class="pageno"><xsl:value-of select="TotalPage"/></span>
              <span class="price"><xsl:value-of select="Price"/></span>
            </li>
          </xsl:for-each>
        </ol>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>