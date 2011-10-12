/*
jQuery XML Parser with Sorting and Filtering
Written by Ben Lister (darkcrimson.com) revised 25 Apr 2010
Tutorial: http://blog.darkcrimson.com/2010/01/jquery-xml-parser/

Licensed under the MIT License:
http://www.opensource.org/licenses/mit-license.php
*/

$(function () {

    function xml_parser(wrapper) {
        $.ajax({
            type: 'GET',
            url: 'books.xml',
            dataType: 'xml',
            success: function (xml_list) {

                var xmlArr = [];
                $(xml_list).find('Book').each(function () {
                    //property fields
                    var xml_isbn = $(this).find('ISBN').text();
                    var xml_title = $(this).find('Title').text();
                    var xml_author = $(this).find('Author').text();
                    var xml_publisher = $(this).find('Publisher').text();
                    var xml_publishedDate = $(this).find('PublishedDate').text();
                    var xml_page = $(this).find('TotalPage').text();
                    var xml_price = $(this).find('Price').text();

                    xmlArr += '<tr>';
                    xmlArr += '<td class="xml_isbn">';
                    xmlArr += xml_isbn;
                    xmlArr += '</td>';

                    xmlArr += '<td class="xml_title">';
                    xmlArr += xml_title;
                    xmlArr += '</td>';

                    xmlArr += '<td class="xml_page">';
                    xmlArr += xml_page;
                    xmlArr += '</td>';

                    xmlArr += '<td class="xml_price">';
                    xmlArr += xml_price;
                    xmlArr += '</td>';
                    xmlArr += '</tr>';
                }); //end loop

                $(xmlArr).appendTo(wrapper + ' table tbody');
            }
        }); //end ajax
    } //end function

    //initialize page
    var wrapper = '#xml_wrapper';
    xml_parser(wrapper);

});