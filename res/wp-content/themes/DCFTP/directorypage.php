<?php /* Template Name: Directory Page */ ?>

<?php get_header(); ?>

<div id="main-section">
	<div class="nav-margin">
	</div>
	
	<!-- Get the navigation links -->
	<?php get_sidebar(); ?>
	
	<div class="main-margin">
	</div>
	
	<div id="main">
		<!-- The content area; restricted width to leave space for sidebar. -->
	
		<div id="page-title">
			<?php single_post_title(); ?>
		</div>
			
		<!-- Get the page content -->
		<?php /* Start the Loop */ ?>
		<?php while ( have_posts() ) : the_post(); ?>
			<?php the_content( ); ?>
		<?php endwhile; ?>
		
		<!-- The map, with controls and search function -->
		
		<div style="text-align: center;">
			<input id="location"></input><br />
			<button onclick="getMap()" class="button">Search</button>
		</div>
		<div id="map-wrapper">
			<div id="map-controls">
				<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/up.png" onclick="zoomIn()"><br />
				<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/down.png" onclick="zoomOut()">
			</div>
			<div id="map">
				<img src = "http://maps.googleapis.com/maps/api/staticmap?center=saddler street durham&zoom=14&size=600x300&maptype=roadmap&sensor=false">
			</div>
		</div>
		
		<div id="entries">
			<!-- The search results would display here -->
		</div>
			
	</div>
	
	<div class="main-margin">
	</div>
</div>
	
<?php get_footer(); ?>