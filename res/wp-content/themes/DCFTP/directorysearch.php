<?php /* Template Name: Full Directory */ ?>

<?php get_header(); ?>

<div id="main-section">
	<div class="nav-margin">
	</div>
	
	<?php get_sidebar(); ?>
	
	<div class="main-margin">
	</div>
	<div id="main">
		<div id="page-title">
			<?php single_post_title(); ?>
		</div>
		<?php /* Start the Loop */ ?>
			<?php while ( have_posts() ) : the_post(); ?>
				<?php the_content( ); ?>
			<?php endwhile; ?>

		<div style="text-align: center;">
			<input id="searchName"></input><br />
			<button onclick="getMap()" class="button">Search</button>
		</div>	
		
		<!-- This should be dynamically generated, so that letters not used don't display -->

		<p style="text-align: center;">
<?PHP
    function starImages($rating) {
        $star = '<img src="/wp-content/plugins/directory/images/star.png" />';
        $grey = '<img src="/wp-content/plugins/directory/images/star_grey.png" />';

        return str_repeat($star, $rating).str_repeat($grey, 3 - $rating);
    }

	$sql = "SELECT * FROM dcftp_directory";
    $data = $wpdb->get_results($sql, ARRAY_A);

    $cats = array();

    foreach ($data as $entry) {
    	$cats[strtolower(substr($entry['name'], 0, 1))] = true;
    }

    echo '<a href="'.$_SERVER["PHP_SELF"].'?page_id='.$_GET['page_id'].'&letter=0-9">0-9</a> ';
	foreach (range('A', 'Z') as $char) {
		echo '| <a href="'.$_SERVER["PHP_SELF"].'?page_id='.$_GET['page_id'].'&letter='.strtolower($char).'">'.$char.'</a> ';
	}
?>
		</p>
		<div id="entries">
<?PHP
	foreach ($data as $entry) {
?>
			<div class="entry">
				<div class="entry-left">
					<div class="entry-name">
						<div class="entry-rating">
							<?PHP echo starImages($entry['stars']); ?>
						</div>
						<?PHP echo $entry['name']; ?>
					</div>
					<p>
						<?PHP echo $entry['information']; ?>
					</p>
				</div>
				<div class="entry-right">
					<p>
						<?PHP echo $entry['addressline1']; ?><br />
						<?PHP echo $entry['addressline2']; ?><br />
						<?PHP echo $entry['city']; ?><br />
						<?PHP echo $entry['postcode']; ?>
					</p>
					<p>
						<?PHP echo $entry['phone']; ?>
					</p>
					<p>
						<a href="<?PHP echo $entry['url']; ?>">Website</a>
					</p>
				</div>
			</div>
<?PHP
	}
?>
		</div>
			
	</div>
	<div class="main-margin">
	</div>
</div>

<?php get_footer(); ?>