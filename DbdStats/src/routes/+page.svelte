<form class="search-container">
	<div class="search-bar">
		<input type="text" name="steaminfo" placeholder="Enter SteamID, custom ID or profile link" />

		<button aria-label="Search" type="submit" />
	</div>
</form>

<style lang="scss">
	$input-background: rgba(68, 68, 68, 0.9);
	$button-background: #555;
	$text-color: #ccc;

	.search-container {
		position: relative;
		width: 100%;
		max-width: 400px;
		padding: 0 0.5em;

		.search-bar {
			height: 2.5em;
			display: flex;
			flex-direction: row;
			border-radius: 500px;
			overflow: hidden;
			box-shadow: 0 0 0px 1px rgba(0, 0, 0, 0.3), 0 2px 30px rgba(0, 0, 0, 0.7);

			&.error {
				box-shadow: 0 0 0px 1px rgba(255, 0, 0, 0.3), 0 0px 10px rgba(255, 47, 47, 0.7);
			}

			&.success {
				box-shadow: 0 0 0px 1px rgba(0, 255, 13, 0.3), 0 0px 10px rgba(71, 255, 47, 0.7);
			}

			input {
				width: 100%;
				border-style: none;
				background-color: $input-background;
				color: $text-color;
				padding-left: 1em;
				font-size: 13px;
				outline: none;

				&::placeholder {
					color: #ccc;
				}
			}

			button {
				border-style: none;
				background-color: $button-background;
				color: $text-color;
				cursor: pointer;
				outline: none;
				width: 3.5em;
				padding: 0;

				.icon {
					font-size: 18px;
					max-width: 18px;
				}

				.loading-container {
					//max-width: 18px;
					display: flex;
					padding: 0.6em;
				}
			}
		}
	}
</style>
