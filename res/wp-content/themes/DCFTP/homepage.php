<?php /* Template Name: Home Page */ ?>

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
		
		<!-- For the homepage, get the side bar with news and key links. -->
		<?php get_template_part( 'homepageAside' ); ?>

		<div id="main-with-aside">
			
			<!-- Get the page content -->
			<?php /* Start the Loop */ ?>
			<?php while ( have_posts() ) : the_post(); ?>
				<?php the_content( ); ?>
				
				<!-- Comments are permanently disabled on the homepage -->
				
			<?php endwhile; ?>
			
		</div>
			
	</div>
	
	<div class="main-margin">
	</div>
</div>
	
<?php get_footer(); ?>