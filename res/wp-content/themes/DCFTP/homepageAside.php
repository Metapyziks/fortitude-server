<div id="aside">
	<div class="padding-10">
		<!-- Home page only. Contains key links and recent news. An 'archive' of blog posts? -->
		<div id="social-buttons">
			<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/twitter-button.png">
			<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/facebook-button.png">
		</div>
		<marquee scrollamount="2" direction="up" loop="true" onmousedown="this.stop();" onmouseup="this.start();">
			<div class="padding-4">
				<?php
					$scroller = new WP_Query('pagename=home/scroller/');
					while($scroller->have_posts()) : $scroller->the_post();
						the_content();
					endwhile;
				?>
			</div>
		</marquee>
					
		<div id="key-links">
			<?php
					$keylinks = new WP_Query('pagename=home/keylinks');
					while($keylinks->have_posts()) : $keylinks->the_post();
						the_content();
					endwhile;
				?>
		</div>
	</div>
</div>