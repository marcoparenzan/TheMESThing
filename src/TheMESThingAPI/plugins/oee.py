# OEE (Overall Equipment Effectiveness) rule. Editable without recompiling the API:
# call POST /api/plugins/oee/reload after saving.
#
#   availability = operating_time / planned_production_time
#   performance  = run_time / operating_time      (net run time vs. time the machine was operating)
#   quality      = good_quantity / total_quantity
#   oee          = availability * performance * quality
#
# A ratio whose denominator is zero is None (not computed), and so is the OEE.


def _ratio(numerator, denominator):
    if denominator == 0:
        return None
    return round(numerator / denominator, 4)


def calculate(planned_production_time_seconds, operating_time_seconds, run_time_seconds,
              good_quantity, total_quantity):
    if min(planned_production_time_seconds, operating_time_seconds, run_time_seconds,
           good_quantity, total_quantity) < 0:
        raise ValueError("times and quantities must be non-negative")
    if operating_time_seconds > planned_production_time_seconds:
        raise ValueError("operating time cannot exceed planned production time")
    if run_time_seconds > operating_time_seconds:
        raise ValueError("run time cannot exceed operating time")
    if good_quantity > total_quantity:
        raise ValueError("good quantity cannot exceed total quantity")

    availability = _ratio(operating_time_seconds, planned_production_time_seconds)
    performance = _ratio(run_time_seconds, operating_time_seconds)
    quality = _ratio(good_quantity, total_quantity)

    oee = None
    if availability is not None and performance is not None and quality is not None:
        oee = round(availability * performance * quality, 4)

    return {
        "availability": availability,
        "performance": performance,
        "quality": quality,
        "oee": oee,
    }
