(function ($) {

    $.fn.googlePlaces = function (options) {
        // This is the easiest way to have default options.
        var settings = $.extend({
            // These are the defaults.
            header: "<h3>Google Reviews</h3>",
            footer: '',
            maxRows: 999,
            minRating: 0,
            months: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
            textBreakLength: "90",
            shortenNames: true,
            placeId: "",
            moreReviewsButtonUrl: '',
            moreReviewsButtonLabel: 'Show More Reviews',
            writeReviewButtonUrl: '',
            writeReviewButtonLabel: 'Write New Review',
            showReviewDate: false,
            showProfilePicture: false
        }, options);

        var targetDiv = this[0];
        var targetDivJquery = this;

        var renderMoreReviewsButton = function () {
            return '<div class="more-reviews"><a href="' + settings.moreReviewsButtonUrl + '" target="_blank">' + settings.moreReviewsButtonLabel + '</a></div>';
        };

        var renderWriteReviewButton = function () {
            return '<div class="write-review"><a href="' + settings.writeReviewButtonUrl + '" target="_blank">' + settings.writeReviewButtonLabel + '</a></div>';
        };

        var renderPicture = function (picture) {
            return "<img class='review-picture' src='" + picture + "'>";
        };

        var renderHeader = function (header) {
            var html = "";
            html += header;
            targetDivJquery.append(html);
        };

        var renderFooter = function (footer) {
            var html = "";
            var htmlButtons = "";

            if (settings.moreReviewsButtonUrl) {
                htmlButtons += renderMoreReviewsButton();
            }
            if (settings.writeReviewButtonUrl) {
                htmlButtons += renderWriteReviewButton();
            }
            if (htmlButtons != "") {
                html += '<div class="buttons">' + htmlButtons + '</div>';
            }

            html += "<br>" + footer + "<br>";
            targetDivJquery.after(html);
        };

        var shortenName = function (name) {
            if (name.split(" ").length > 1) {
                var shortenedName = "";
                shortenedName = name.split(" ");
                var lastNameFirstLetter = shortenedName[1][0];
                var firstName = shortenedName[0];
                if (lastNameFirstLetter == ".") {
                    return firstName;
                } else {
                    return firstName + " " + lastNameFirstLetter + ".";
                }
            } else if (name != undefined) {
                return name;
            } else {
                return '';
            }
        };

		var renderStars = function (rating) {
			var stars = '<div class="rating"><div style="width:' + rating * 20 + '%;"></div></div>';
            return stars;
        };

        var convertTime = function (UNIX_timestamp) {
            var newDate = new Date(UNIX_timestamp * 1000);
			var time = newDate.toLocaleDateString('en-US', { year: 'numeric', month: '2-digit', day: '2-digit' });
            return time;
        };

        var filterReviewsByMinRating = function (reviews) {
            if (reviews === void 0) {
                return [];
            } else {
                for (var i = reviews.length - 1; i >= 0; i--) {
                    var review = reviews[i];
                    if (review.rating < settings.minRating) {
                        reviews.splice(i, 1);
                    }
                }
                return reviews;
            }
        };

        var sortReviewsByDateDesc = function (reviews) {
            if (typeof reviews != "undefined" && reviews != null && reviews.length != null && reviews.length > 0) {
                return reviews.sort(function (a, b) { return (a.time > b.time) ? 1 : ((b.time > a.time) ? -1 : 0); }).reverse();
            } else {
                return []
            }
        }

		var renderReviews = function (reviews) {
            var reviews = GetDataFromGoogle();

            reviews.reverse();
            var html = "<div class='blog-posts'>";
            var rowCount = (settings.maxRows > 0) ? settings.maxRows - 1 : reviews.length - 1;
            rowCount = (rowCount > reviews.length - 1) ? reviews.length - 1 : rowCount;
            for (var i = rowCount; i >= 0; i--) {
                var review = reviews[i];
                var stars = renderStars(review.rating);
                var date = convertTime(review.time);
                var name = settings.shortenNames ? shortenName(review.author_name) : review.author_name;
                var style = (review.text.length > parseInt(settings.textBreakLength)) ? "review-item-long" : "review-item";

                var picture = "";
                if (settings.showProfilePicture) {
                    picture = renderPicture(review.profile_photo_url);
                }

                html += "<div class=\"three-items-holder\">" +
                    "<div class=\"blog-post item-box\" style=\"border:none;\">" +
                    "<div class=\"product-item\" style=\"border:none;\">" +
                    "<div class=\"details\">" +
                    "<div class=\"product-review-item\">" +
                    "<div class=\"review-item-head\">" +
                    "<div class=\"product-review-box\">" +
                    stars +
                    "</div>" +
                    "<div class=\"review-info\">" +
                    "<span class=\"date\">Published " +
                    "<strong>" + date + "</strong>" +
                    "</span>" +
                    "<span class=\"user\"> by " +
                    "<strong >" + name +"</strong>" +
                    "</span>" +
                    "</div>" +
                    "</div>" +
                    "<div class=\"review-text\">" +
                    review.text +
                    "</div>" +
                    "</div>" +
                    "</div>" +
                    "</div>" +
                    "</div>" +
                    "</div>";
            }
            html += "</div>";
            targetDivJquery.append(html);
        };

        var handleHomePageReviewCarousel = function () {
            var richBlogCarousel = $('.rich-blog-homepage.homepage-review .blog-posts');

            if (richBlogCarousel.length === 0) {
                return;
            }

            var blogPosts = richBlogCarousel.children('.blog-post');

            richBlogCarousel.owlCarousel({
                rtl: $('body').hasClass('rtl'),
                loop: true,
                autoPlay: true,
                autoplayHoverPause: true,
                nav: true,
                dots: false,
                //rewind: true,
                margin: 20,
                responsive: {
                    0: {
                        items: 1
                    },
                    1001: {
                        items: 4
                    }
                },
                onInitialize: function (event) {
                    if (blogPosts.length <= 1) {
                        this.settings.loop = false;
                    }
                }
            });
        };
        // GOOGLE PLACES API CALL STARTS HERE

        // initiate a Google Places Object
        var service = new google.maps.places.PlacesService(targetDiv);
        // set.getDetails takes 2 arguments: request, callback
        // see documentation here:  https://developers.google.com/maps/documentation/javascript/3.exp/reference#PlacesService
        const request = {
            placeId: settings.placeId
        };
        // the callback is what initiates the rendering if Status returns OK
        var callback = function (place, status) {
            if (status == google.maps.places.PlacesServiceStatus.OK) {
                var filteredReviews = filterReviewsByMinRating(place.reviews);
                var sortedReviews = sortReviewsByDateDesc(filteredReviews);
                if (sortedReviews.length > 0) {
                    renderHeader(settings.header);
                    renderReviews(sortedReviews);
                    renderFooter(settings.footer);
                    handleHomePageReviewCarousel();
                }
            }
        }

        function GetDataFromGoogle() {
			var data = [
				{
					"author_name": "Catherine Gilkey ",
					"rating": 5,
					"relative_time_description": "1 month ago",
					"time": 1647620629,
					"text": "My order came in early, and the quality was better than expected! Don't hesitate to use this company! Also, they're great about communicating your order status through email. Kudos to Richard, their all-star employee, for his help with my order! ",
					"language": "en"
				},
				{
					"author_name": "Chris Martin ",
					"rating": 5,
					"relative_time_description": "2 months ago",
					"time": 1642869789,
					"text": "Can't say how much I appreciate the customer service from Richard Wallace as well as the ease of purchase, customization, price and quantity of the products for my small business jiujitsuinsurance.com. Thank you all! ",
					"language": "en"
				},
				{
					"author_name": "David Blackburn ",
					"rating": 5,
					"relative_time_description": "9 months ago",
					"time": 1624838400,
					"text": "The pens are exactly as advertised and the logo printing was precise and clear. Terrific team to work with!! Thanks!! ",
					"language": "en"
				},
				{
					"author_name": "Domaine Serene (DS Hospitality) ",
					"rating": 4,
					"relative_time_description": "9 months ago",
					"time": 1623888000,
					"text": "Always professional and accommodating to needs. The shipping is prompt and the product is as expected. ",
					"language": "en"
				},
				{
					"author_name": "Elizabeth Oliva ",
					"rating": 5,
					"relative_time_description": "9 months ago",
					"time": 1623456000,
					"text": "I have ordered personalized pens for the graduating class at my school for the last 4-5 years and Save Your Ink has been awesome to work with. They are quick to respond to any inquiries. Ordering is so easy...they save my design on file and update it with the new year. Of all the things I plan and order for the end of the year - Save Your Ink is by far the easiest and most reliable company that I work with!! ",
					"language": "en"
				},
				{
					"author_name": "Ginger Dodge ",
					"rating": 5,
					"relative_time_description": "9 months ago",
					"time": 1623369600,
					"text": "Great customer service with very good follow up and quick turn arounds on proofs and orders. Our order arrived quickly and in good shape and the color imprints look great on these pens. ",
					"language": "en"
				},
				{
					"author_name": "Val Stoochnoff ",
					"rating": 5,
					"relative_time_description": "9 months ago",
					"time": 1623024000,
					"text": "The pens look great! ",
					"language": "en"
				},
				{
					"author_name": "Mark Reynolds ",
					"rating": 5,
					"relative_time_description": "10 months ago",
					"time": 1621036800,
					"text": "Can not express how grateful as a small business owner that needs accessories of how grateful I'm that I found Save your Ink through Google. Richard and his Team do amazing customer service and always puts his customers first. ",
					"language": "en"
				},
				{
					"author_name": "Office St. Peter's Niagara Falls ",
					"rating": 5,
					"relative_time_description": "10 months ago",
					"time": 1620000000,
					"text": "Great customer service and order quickly shipped! ",
					"language": "en"
				},
				{
					"author_name": "TJ Potter ",
					"rating": 5,
					"relative_time_description": "11 months ago",
					"time": 1619136000,
					"text": "Extremely helpful customer service, was able to fulfill a big pen request in a very short amount of time. Highly recommend! ",
					"language": "en"
				},
				{
					"author_name": "Beverly Long ",
					"rating": 5,
					"relative_time_description": "11 months ago",
					"time": 1618704000,
					"text": "Save Your Ink is great! They do great work in a timely manner. Richard was able to get our products done and shipped to us in time for our event with very short notice when no other company I checked with was able to do it. Thanks so much Richard and Save Your Ink! ",
					"language": "en"
				},
				{
					"author_name": "Laura Wilson ",
					"rating": 5,
					"relative_time_description": "11 months ago",
					"time": 1618358400,
					"text": "Knives were exactly what we expected, The service was great and the assistance from Richard was awesome. We are totally happy with everything. Thanks! ",
					"language": "en"
				},
				{
					"author_name": "Rachel Duchac ",
					"rating": 5,
					"relative_time_description": "11 months ago",
					"time": 1618272000,
					"text": "We had a great experience! Customer service was extremely helpful, and the products turned out perfectly! Thank you! ",
					"language": "en"
				},
				{
					"author_name": "Paula Radano ",
					"rating": 5,
					"relative_time_description": "11 months ago",
					"time": 1618099200,
					"text": "Thank you for the opportunity to express how grateful we are to have found your company, \"My Promotional Pens.\" As in previous orders, the attentiveness to our requirements were superb. Delivery was also on time. ",
					"language": "en"
				},
				{
					"author_name": "Elvira Cedillo ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1618012800,
					"text": "I am amazed how quickly we received our items and with the quality of our pens and koozies, very impressive. Thank you Bill who made all this happen. I definitely recommend the company. ",
					"language": "en"
				},
				{
					"author_name": "April Davis ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1617235200,
					"text": "This company was great to work with, very quick to respond to any inquiries, questions, etc. The products that order were great and arrived in the promised amount of time! ",
					"language": "en"
				},
				{
					"author_name": "kelly trantham ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1616889600,
					"text": "It was a pleasure working with Richard. Our pens were perfect. Not only do they look great, they also write beautifully. We will definitely contact them in the future for more orders. ",
					"language": "en"
				},
				{
					"author_name": "Nancy Huber ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1616716800,
					"text": "I own Leonard Printing in Brookhaven, Ms we print on paper not pens so I use this company for my company's advertising pens. They are A pleasure to do business with. I would highly recommend this company. Great value and quality. Will be buying again. ",
					"language": "en"
				},
				{
					"author_name": "Brandy Mefford ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1616284800,
					"text": "Ordered a pen order on a rush. Pens were delivered before the deadline and were great quality pens. The imprint on each pen was great. Awesome to work with. Will order again from this company. Super nice to work with, professional and hits the deadline before the deadline. ",
					"language": "en"
				},
				{
					"author_name": "Scott Gleason ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1616198400,
					"text": "I ordered 500 key tags from Richard and had a great experience. I didn't have a logo file, so he created one for me and did a great job. I would definitely recommend Save Your Ink, Inc for any promotional product needs you may have. ",
					"language": "en"
				},
				{
					"author_name": "Paul Howarth ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1615939200,
					"text": "Quick and reliable service. The pens worked great and were just what we wanted. ",
					"language": "en"
				},
				{
					"author_name": "DavidandJanet Rogers gmail ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1615852800,
					"text": "Great experience. Richard was very helpful! Received just what we ordered and in quicker time than we thought ",
					"language": "en"
				},
				{
					"author_name": "Trinity Lutheran Church Stephens City VA ",
					"rating": 4,
					"relative_time_description": "1 year ago",
					"time": 1615766400,
					"text": "Very good customer service. Attentive but not pushy. Product arrived quickly and looked exactly as expected. ",
					"language": "en"
				},
				{
					"author_name": "Catsifier Team ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1615507200,
					"text": "Every time we order pens, they come out more than perfect! They always get them to us very quickly and they never disappoint. Would recommend them any day! ",
					"language": "en"
				},
				{
					"author_name": "Olaa Canada ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1615334400,
					"text": "The management of Save Your Ink puts the serve in service. My order of beautiful and functional stylus pens came early and with the human input of Richard and his team all the way. Thank you for the rates and quality... My goal was met! ",
					"language": "en"
				},
				{
					"author_name": "michael guleserian ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1615334400,
					"text": "Great Customer service!!! ",
					"language": "en"
				},
				{
					"author_name": "Michelle Peterson ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1615161600,
					"text": "Great company! They have worked hard with us to point us to quality products and help us design excellent imprints which look good and convey our messages. We give Save Your Ink, Inc. our highest recommendation! ",
					"language": "en"
				},
				{
					"author_name": "J.C. Webb ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1615161600,
					"text": "Great customer service and turnaround. Item got delayed from original due date, but customer service jumped on it and expedited the order after contacting them. Competitive pricing ",
					"language": "en"
				},
				{
					"author_name": "Alex Winslow ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1614988800,
					"text": "Incredibly responsive and great quality product! They rushed an order for me with 24 hours notice and the communication was impeccable. ",
					"language": "en"
				},
				{
					"author_name": "Michelle Madden ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1614816000,
					"text": "Great customer service. Richard was very prompt in responses and our pens turned out great! ",
					"language": "en"
				},
				{
					"author_name": "Sat Jivan Singh Khalsa ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1614556800,
					"text": "I have been buying the Knight Marble pens for about the last 15 years. They are very similar to Mont Blanc pens and my clients love them. In my law firm, we always provide our clients the pens to sign their legal documents and then give them as a gift thereafter. Clients are very appreciative and hang on to them for a long time. They are the best. I highly recommend them. ",
					"language": "en"
				},
				{
					"author_name": "Joe Lee ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1614297600,
					"text": "Great products, excellent service. ",
					"language": "en"
				},
				{
					"author_name": "Beth Hawks ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1614124800,
					"text": "We purchased some custom pens through this organization and LOVE them! The pen itself is high quality, well made, and reliable. Our logo looks great. Seller had awesome communication and the shipping was fast. We had a great experience. ",
					"language": "en"
				},
				{
					"author_name": "Jina Hardy ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1614124800,
					"text": "Beautiful tumbler - great gift idea. The box is a stellar items to have at all our job fairs! ",
					"language": "en"
				},
				{
					"author_name": "Julie Abouzelof ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1614038400,
					"text": "I purchase pens a couple of times a year to give away to customers. They're simple, attractive and great for advertising. My Promotional Pens gets them delivered fast and for a good price. Their customer service is great too. I've been a satisfied customer for 5+ years ",
					"language": "en"
				},
				{
					"author_name": "Donna Thompson ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1613520000,
					"text": "Great products at a great price! ",
					"language": "en"
				},
				{
					"author_name": "Lesia Mckinnon ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1612224000,
					"text": "I always get quick, courteous service and great prices. Pens look great and write well and are very comfortable in your hand. No duds in the mix. Thanks for a great product. I will use again for future orders. ",
					"language": "en"
				},
				{
					"author_name": "Megan Laurie ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1611533578,
					"text": "This company makes it so easy to order and re-order our pens. They provide great one on one customer service and fair competitive pricing. We will be buying our company pens here forever. ",
					"language": "en"
				},
				{
					"author_name": "Tenesha Batiste ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1610842345,
					"text": "Such an easy process! This company is great. They not only do what you request but offer great suggestions and is timely! Would use again. They have a customer forever. ",
					"language": "en"
				},
				{
					"author_name": "Dan Junker ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1610236987,
					"text": "We are very happy with our order of promotional pens. Great communication, personalized service and very fast shipping! ",
					"language": "en"
				},
				{
					"author_name": "Madison Price ",
					"rating": 5,
					"relative_time_description": "1 year ago",
					"time": 1610064456,
					"text": "Richard has always been incredibly helpful getting me what I need in a pinch. He works super hard to be as competitive price wise as possible and never fails me on delivery lead times or quality of product. Highly recommend!!! ",
					"language": "en"
				}
			];
            return data;
        }

        return this.each(function () {
            // Runs the Plugin
            if (settings.placeId === undefined || settings.placeId === "") {
                console.error("NO PLACE ID DEFINED");
                return
            }
            service.getDetails(request, callback);
        });
    };

}(jQuery));
