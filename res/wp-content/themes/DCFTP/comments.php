<?php
	if(!empty($_SERVER['SCRIPT_FILENAME']) && 'comments.php' == basename($_SERVER['SCRIPT_FILENAME']))
		die ('Please do not load this page directly. Thanks!');
		
	if( post_password_required() ) { ?>
		<p>
			This post is password protected. Enter the password to view comments.
		</p>
	<?php
		return;
	}
?>

<div id="comments">
	
	<?php if ( have_comments() ) : ?>
		
		<ol class="commentlist">
			<?php foreach ($comments as $comment) : ?>
				
				<li>
					
					<div class="commenttext">
						<cite>
							<?php comment_author_link() ?>
						</cite>
						
						<?php comment_text() ?>
						<div style="float: right;">
							<?php edit_comment_link('Edit','&nbsp;&nbsp;',''); ?>
						</div>
						<span class="date">
							<a href="#comment-<?php comment_ID() ?>" title=""><?php comment_time() ?> on <?php comment_date('n/j/y') ?></a>
						</span>
						
						
					</div>
					
					<?php if ($comment->comment_approved == '0') : ?>
						<div class="awaiting_moderation">
							Your comment is awaiting moderation.
						</div>
					<?php endif; ?>
				</li>			
			<?php endforeach; /* end for each comment */ ?>
		</ol>
		
		<?php if ($wp_query->max_num_pages > 1) : ?>
			<div class="pagination">
				<ul>
					<li class="older"><?php previous_comments_link('Older') ?></li>
					<li class="newer"><?php next_comments_link('Newer') ?></li>
				</ul>
			</div>
		<?php endif; ?> <!-- end if pagination -->
		
	<?php endif; ?> <!-- end if have comments -->
	
	<?php if (comments_open() ) : ?>
	
		<div id="leaveComment">
			<div id="respondPadding">
				<span id="respondLeaveComment">Leave a comment?</span>
				<?php comment_form(); ?>	
			</div>
		</div>
	<?php else : ?>
		<p> Comments are now closed.</p>		
	<?php endif; ?> <!-- Comments are open -->
	
</div>