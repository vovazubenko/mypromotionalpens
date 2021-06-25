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
            var stars = '<div class="review-stars"><ul>';
            // fills gold stars
            for (var i = 0; i < rating; i++) {
                stars += '<li><i class="star"></i></li>';
            }
            // fills empty stars
            if (rating < 5) {
                for (var i = 0; i < (5 - rating); i++) {
                    stars += '<li><i class="star inactive"></i></li>';
                }
            }
            stars += "</ul></div>";
            return stars;
        };

        var convertTime = function (UNIX_timestamp) {
            var newDate = new Date(UNIX_timestamp * 1000);
            var months = settings.months;
            var time = newDate.getMonth() + "/" + newDate.getDate() + "/" + newDate.getFullYear();
            // var time = newDate.getDate() + ". " + months[newDate.getMonth()] + " " + newDate.getFullYear();
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
                    "<div class=\"rating\">" +
                    "<div>" +
                    stars +
                    "</div>" +
                    "</div>" +
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
