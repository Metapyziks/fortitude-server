$(function() {
alert ('Hello world!');
$('#content').find('*').addClass('deselected');
$('#content li').bind('click',function(e) { $(e.target).toggleClass('deselected'); } );

});