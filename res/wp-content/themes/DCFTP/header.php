<!DOCTYPE html>
<html>
<head>
	<title><?php
	/*
	 * Print the <title> tag based on what is being viewed.
	 */
	global $page, $paged;

	wp_title( '|', true, 'right' );

	// Add the blog name.
	bloginfo( 'name' );

	// Add the blog description for the home/front page.
	$site_description = get_bloginfo( 'description', 'display' );
	if ( $site_description && ( is_home() || is_front_page() ) )
		echo " | $site_description";

	// Add a page number if necessary:
	if ( $paged >= 2 || $page >= 2 )
		echo ' | ' . sprintf( __( 'Page %s', 'twentyeleven' ), max( $paged, $page ) );

	?></title>
	
	<link rel="stylesheet" type="text/css" media="all" href="<?php bloginfo( 'stylesheet_url' ); ?>" />
	
	<script type="text/javascript" src="<?php bloginfo('stylesheet_directory'); ?>/script.js"></script>
	
	<!-- Enable threaded comments -->
	<?php if ( is_singular() ) wp_enqueue_script( 'comment-reply' ); ?>
	
	<?php wp_head(); ?>
</head>

<body <?php body_class(); ?>>
<div id="wrapper">
	<div id="header">
		<div class="header-image-landscape-wrapper">
			<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/Sky.jpg">
		</div>
		<div class="header-image-portrait">
			<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/cotton-man.jpg">
		</div>
		<div class="header-image-landscape-wrapper">
			<div class="header-image-landscape">
				<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/olive-men.jpg">
			</div>
			<div class="header-image-landscape">
				<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/coffee-woman.jpg">
			</div>
		</div>
		<div class="header-image-portrait">
			<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/coffee-man.jpg">
		</div>
		<div class="header-image-landscape-wrapper">
			<div class="header-image-landscape">
				<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/banana-man.jpg">
			</div>
			<div class="header-image-landscape">
				<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/tea-woman.jpg">
			</div>
		</div>
		<?php wp_head(); ?>
	</div>
	<div id="site-title">
		<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/title.png">
	</div>
	<div id="nav-top">
		<div class="padding-10">
			<!-- Logo goes here -->
			<img src="<?php bloginfo('stylesheet_directory'); ?>/layoutImages/logo.jpg">
		</div>
	</div>