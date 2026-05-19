function setId(val)
{
	$('#inst_id').val(val);
	$('#overlay').css('height',$('#popupbody').height+'px');
	$('#instData').hide();
	$('#frmdetails')[0].reset();
	$('#mheader').html('Enter Your Details');
	$('#frmdetails').show();
	$('#frmsubmit').show();
	$('#myModal').modal();
}

function getInstPhone()
{
	if(!$('#frmdetails').valid())
		alert('Form not valid');
	else{
		$('#overlay').show();
		var url = $('#frmdetails').attr('action');
		var posting = $.post(url, $('#frmdetails').serialize());
		
		posting.done(function( data ) {
           $('#instData').html(data).show();
		   $('#mheader').html('Contact Details of Institute');
		   $('#frmdetails').hide();
		   $('#frmsubmit').hide();
		   $('#overlay').hide();
        });
	}
	return false;
}