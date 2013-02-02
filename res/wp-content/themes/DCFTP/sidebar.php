<div id="nav">
	<div class="padding-10">
	
		<ul id="nav-ul">
					
			<!-- Display links to all pages with no navigation title -->
			<!-- 67 and 63 refer to the key links and scrolling news -->
			<?php wp_list_pages('title_li=&exclude=67,63' ); ?>
			
			<!-- The link to the dashboard - only displayed to logged in users. -->
			<?php wp_register(); ?>
			
			<!-- The login link, displayed to all users. Needs <li> to display inline. -->
			<li>
				<?php wp_loginout(); ?>
			</li>
		</ul>
		
	</div>
</div>