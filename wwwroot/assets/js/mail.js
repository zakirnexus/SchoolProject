<script type="text/javascript">
$(document).ready(function(){
$(".modal-backdrop.fade.in").hide();

var x = parseQuerystring();
if(x.coll != null){
	$("#msg_popup").css('display','block');
	$(".modal-backdrop.fade.in").show();
	$("#msg_content").append("Your message has been successfully sent to "+x.coll);
} 


$(".btn_Enquiry").click(function(e){
e.preventDefault();
	$("#page_URL").attr('value',window.location.href+"?sucess=yes&coll="+$(this).parent().find(".collge_name").attr('value'));
	$("#myModal").css('display','block');
	$("#myModal").attr('aria-hidden','true');
	$("#myModal").addClass('in');
	$(".modal-backdrop.fade.in").show();
	$("#rcpt_val").attr('value',$(this).parent().find(".collge_name").attr('value'));
	$("#classfn_val").attr('value',$(this).attr('data-classfn'));
});	
	
$("#close_form").click(function(){
	$("#myModal").css('display','none');
	$("#myModal").attr('aria-hidden','false');
	$("#myModal").removeClass('in');
	$(".modal-backdrop.fade.in").hide();
	$("#msg_popup").css('display','none');
	
});

});
function parseQuerystring(){
    var foo = window.location.href.split('?')[1].split('#')[0].split('&');
    var dict = {};
    var elem = [];
    for (var i = foo.length - 1; i >= 0; i--) {
        elem = foo[i].split('=');
        dict[elem[0]] = elem[1];
    };
    return dict;
};

</script>